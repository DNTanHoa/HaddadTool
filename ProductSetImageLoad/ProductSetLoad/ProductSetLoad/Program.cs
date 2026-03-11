using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.SqlClient;

namespace ProductSetLoad
{
    internal class Program
    {
        // ===== CONFIG =====
        static string SOURCE_DIR = @"\\125.212.216.10\LeadingstarSMB\Public\BU1\HADDAD\ERP - product type";
        static string DEST_DIR = @"C:\Docker\erp-app\volumes\erp-sale-api\wwwroot"; // folder api-sales serve static
        static string CUSTOMER_ID = "HA";
        static string BASE_IMAGE_URL = "https://api-sales.erpleadingstar.com/";

        static string CONNECTION_STRING =
            "Server=172.16.0.5,13433;Database=ERPv2_Production;User Id=etl;Password=001Le@ding@2025@)@@@;TrustServerCertificate=True;";

        static readonly HashSet<string> VALID_EXTS = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".webp"
        };

        static readonly Dictionary<string, int> EXT_PRIORITY = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { ".png", 0 }, { ".jpg", 1 }, { ".jpeg", 2 }, { ".webp", 3 }
        };

        static void Main(string[] args)
        {
            if (!Directory.Exists(SOURCE_DIR))
                throw new DirectoryNotFoundException("SOURCE_DIR not found: " + SOURCE_DIR);

            if (!Directory.Exists(DEST_DIR))
                Directory.CreateDirectory(DEST_DIR);

            if (!BASE_IMAGE_URL.EndsWith("/"))
                BASE_IMAGE_URL += "/";

            // 1) Scan images
            List<string> imgFiles = Directory.EnumerateFiles(SOURCE_DIR)
                .Where(f => VALID_EXTS.Contains(Path.GetExtension(f)))
                .ToList();

            if (imgFiles.Count == 0)
            {
                Console.WriteLine("No images found.");
                return;
            }

            // 2) Pick best per style
            Dictionary<string, string> bestByStyle = PickBestFilePerStyle(imgFiles);

            // 3) Only ProductSet missing ImageUrl
            List<string> styles = bestByStyle.Keys.ToList();
            Dictionary<string, long> missing = GetMissingProductSetIds(CUSTOMER_ID, styles);

            if (missing.Count == 0)
            {
                Console.WriteLine("Nothing to do: no ProductSet missing ImageUrl.");
                return;
            }

            int copied = 0, copySkipped = 0, dbUpdated = 0, dbSkipped = 0, failed = 0;

            foreach (KeyValuePair<string, long> kv in missing)
            {
                string style = kv.Key;
                long id = kv.Value; // not used, but kept for consistency/debugging

                string srcPath;
                if (!bestByStyle.TryGetValue(style, out srcPath))
                    continue;

                string fileName = Path.GetFileName(srcPath);
                string destPath = Path.Combine(DEST_DIR, fileName);

                try
                {
                    bool didCopy = false;

                    if (!File.Exists(destPath))
                    {
                        File.Copy(srcPath, destPath, false);
                        didCopy = true;
                        copied++;
                    }
                    else
                    {
                        copySkipped++;
                    }

                    string newUrl = BASE_IMAGE_URL + fileName;
                    int u = UpdateImageUrlIfEmpty(CUSTOMER_ID, style, newUrl);

                    if (u > 0)
                    {
                        dbUpdated += u;
                        Console.WriteLine("[OK] style=" + style + " id=" + id + " file=" + fileName + " copied=" + didCopy + " db_updated=" + u);
                    }
                    else
                    {
                        dbSkipped++;
                        Console.WriteLine("[SKIP-DB] style=" + style + " id=" + id + " file=" + fileName + " copied=" + didCopy + " (ImageUrl already set?)");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.WriteLine("[FAIL] style=" + style + " id=" + id + " file=" + fileName + " error=" + ex.Message);
                }
            }

            Console.WriteLine();
            Console.WriteLine("=== SUMMARY ===");
            Console.WriteLine("To process (missing ImageUrl): " + missing.Count);
            Console.WriteLine("Copied new files: " + copied);
            Console.WriteLine("Copy skipped (already exists): " + copySkipped);
            Console.WriteLine("DB updated: " + dbUpdated);
            Console.WriteLine("DB skipped: " + dbSkipped);
            Console.WriteLine("Failed: " + failed);
        }

        static Dictionary<string, string> PickBestFilePerStyle(List<string> paths)
        {
            Dictionary<string, string> best = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string p in paths)
            {
                string style = (Path.GetFileNameWithoutExtension(p) ?? "").Trim();
                if (style.Length == 0) continue;

                string ext = Path.GetExtension(p);

                string oldPath;
                if (!best.TryGetValue(style, out oldPath))
                {
                    best[style] = p;
                    continue;
                }

                string oldExt = Path.GetExtension(oldPath);
                int prNew = EXT_PRIORITY.ContainsKey(ext) ? EXT_PRIORITY[ext] : 99;
                int prOld = EXT_PRIORITY.ContainsKey(oldExt) ? EXT_PRIORITY[oldExt] : 99;

                if (prNew < prOld)
                    best[style] = p;
            }

            return best;
        }

        static Dictionary<string, long> GetMissingProductSetIds(string customerId, List<string> styles)
        {
            Dictionary<string, long> result = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            const int CHUNK = 800;

            using (SqlConnection conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                for (int i = 0; i < styles.Count; i += CHUNK)
                {
                    List<string> chunk = styles.Skip(i).Take(CHUNK).ToList();
                    if (chunk.Count == 0) continue;

                    List<string> paramNames = chunk.Select((s, idx) => "@s" + idx).ToList();

                    string sql =
                        "SELECT CustomerStyle, Id " +
                        "FROM ERPv2_Production.dbo.ProductSet " +
                        "WHERE CustomerId = @CustomerId " +
                        "  AND CustomerStyle IN (" + string.Join(",", paramNames) + ") " +
                        "  AND (ImageUrl IS NULL OR LTRIM(RTRIM(ImageUrl)) = '')";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerId", customerId);

                        for (int k = 0; k < chunk.Count; k++)
                            cmd.Parameters.AddWithValue(paramNames[k], chunk[k]);

                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                string style = rd.GetString(0);
                                long id = rd.GetInt64(1); // BIGINT => Int64
                                if (!result.ContainsKey(style))
                                    result.Add(style, id);
                            }
                        }
                    }
                }
            }

            return result;
        }

        static int UpdateImageUrlIfEmpty(string customerId, string style, string imageUrl)
        {
            string sql =
                "UPDATE ERPv2_Production.dbo.ProductSet " +
                "SET ImageUrl = @ImageUrl, " +
                "    LastUpdatedAt = DATEADD(HOUR, 7, SYSUTCDATETIME()) " +
                "WHERE CustomerId = @CustomerId " +
                "  AND CustomerStyle = @CustomerStyle " +
                "  AND (ImageUrl IS NULL OR LTRIM(RTRIM(ImageUrl)) = '')";

            using (SqlConnection conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ImageUrl", imageUrl);
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);
                    cmd.Parameters.AddWithValue("@CustomerStyle", style);
                    return cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
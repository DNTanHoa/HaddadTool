using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Windows.Forms;
using SeleniumExtras.WaitHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.IO;
using System.ComponentModel;
using DevExpress.Data.NetCompatibility.Extensions;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Diagnostics.Contracts;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Threading;
using DevExpress.XtraEditors.Filtering;
using System.Reflection.Emit;
//using static DevExpress.Data.Filtering.Helpers.SubExprHelper.ThreadHoppingFiltering;

namespace Photo8
{
    public partial class Form1 : Form
    {
        private IWebDriver driver;
        private int CheckProcess;
        private DataTable dt = new DataTable();
        public Form1()
        {
            InitializeComponent();
            CheckProcess = 0;
            Input_link.Text = Properties.Settings.Default.Image;
            //rtbPO.Text = "0616473, 0616474, 0616475, 0616477, 0616478, 0616479, 0616480, 0616481, 0616482, 0616483, 0616484, 0616485, 0616486, 0616487, 0616488, 0616489, 0616490, 0616492, 0616493, 0616494, 0616495, 0616496, 0616497, 0616498, 0616499, 0616500, 0616501, 0616502, 0616503, 0616504, 0616505, 0616506, 0616507, 0616508, 0616509, 0616510, 0616511, 0616512, 0616513, 0616514, 0616515, 0616516, 0616517, 0616518, 0616519, 0616520, 0616521, 0616522, 0616523, 0616524, 0616525, 0616526, 0616527, 0616528, 0616529, 0616530";
            txtType.Text = Properties.Settings.Default.Type;
            txtContractor.Text = Properties.Settings.Default.Contractor;
            txtSeason.Text = Properties.Settings.Default.Season;
        }
        private string GetChromeProfilePath()
        {
            // Đã có path → dùng luôn
            if (!string.IsNullOrEmpty(Properties.Settings.Default.ChromeProfilePath)
                && Directory.Exists(Properties.Settings.Default.ChromeProfilePath))
            {
                return Properties.Settings.Default.ChromeProfilePath;
            }

            // Chưa có → hỏi user
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Chọn thư mục lưu Chrome Selenium Profile";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() != DialogResult.OK)
                    throw new Exception("Chưa chọn thư mục lưu Chrome profile.");

                // Tạo thư mục ChromeSeleniumProfile bên trong
                string profilePath = Path.Combine(dialog.SelectedPath, "ChromeAuthProfile");
                Directory.CreateDirectory(profilePath);

                // Lưu setting
                Properties.Settings.Default.ChromeProfilePath = profilePath;
                Properties.Settings.Default.Save();

                return profilePath;
            }
        }
        private void SelectFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Chọn folder chứa hình ảnh";
                //fbd.UseDescriptionForTitle = true;
                fbd.ShowNewFolderButton = false;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = fbd.SelectedPath;
                    Input_link.Text = folderPath;
                }
            }
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string profilePath = GetChromeProfilePath();

                if (!Directory.Exists(profilePath))
                    Directory.CreateDirectory(profilePath);

                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--start-maximized");
                options.AddArgument($"--user-data-dir={profilePath}");

                driver = new ChromeDriver(options);

                // Điều hướng tới trang login
                driver.Navigate().GoToUrl("https://photo8.haddad.com/");
                lblStatus.Text = "Status: Login Sucess!";
                CheckProcess = 1;
                CheckProcess = 2;
            }
            catch (Exception ex)
            {
                if (driver != null)
                {
                    driver.Quit();
                    driver = null;
                    lblStatus.Text = "Status: Login Fail!";
                }
                CheckProcess = 0;
                MessageBox.Show(ex.Message);
            }
        }

        private void btnLoadTasks_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra đã login
                if (driver == null)
                {
                    MessageBox.Show("Bạn chưa đăng nhập!");
                    return;
                }

                string rawInputLb = rtbLabel.Text;
                string rawInputPO = rtbPO.Text;
                string ContractNo = txtContractNo.Text;
                string Contractor = txtContractor.Text;
                string Season = txtSeason.Text;

                // Cho phép ngăn cách bằng: , hoặc xuống dòng
                var LabelList = rawInputLb
                    .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();
                var POList  = rawInputPO
                    .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();
                if ((LabelList.Count == 0 || string.IsNullOrWhiteSpace(ContractNo)
                    || string.IsNullOrWhiteSpace(Contractor)
                    || string.IsNullOrWhiteSpace(Season)
                    ) && POList.Count == 0)
                {
                    MessageBox.Show("Vui lòng nhập đủ Contractor-ContractNo-Season-Label hoặc nhập theo PO");
                    return;
                }

                lblStatus.Text = "Status: Processing...";

                if (POList.Count == 0)
                {
                    var parameters = LabelList
                        .Select((Label, index) => $"@p{index}")
                        .ToList();

                    string sql = $@"
                    SELECT
                        PurchaseOrderNumber
                        ,CASE WHEN Season = 'M' THEN 'Summer '
                              WHEN Season = 'F' THEN 'Fall '
                              WHEN Season = 'S' THEN 'Spring '
                              WHEN Season = 'H' THEN 'Holiday '
                            END + cast(Year as nvarchar(4)) Season
                        ,ContractNo
                        ,CustomerStyle
                        ,FORMAT(ShipDate, 'MMM dd, yyyy', 'en-US') AS ShipDateText
                        ,ColorCode
                        ,LabelCode
                        ,Account
                        ,Contractor
                        ,Length
                        ,Width
                        ,Height
                        ,NetWeight
                        ,GrossWeight
                        ,QuantityPerCarton
                    FROM ItemStyle i
                    LEFT JOIN 
                    (
                        SELECT 
	                        LSStyle,MAX(TRY_CAST(Length as float)) Length
	                        ,MAX(TRY_CAST(Width as float)) Width
	                        ,MAX(TRY_CAST(Height as float)) Height
	                        ,MAX(TRY_CAST(NetWeight as float)) NetWeight
	                        ,MAX(TRY_CAST(GrossWeight as float)) GrossWeight
	                        ,MAX(TRY_CAST(QuantityPerCarton as float)) QuantityPerCarton
                        FROM PackingLine
                        WHERE IsDeleted = 0
                        GROUP BY LSStyle
                    )p ON p.LSStyle = i.LSStyle AND i.IsDeleted = 0 AND ISNULL(ItemStyleStatusCode,1) <> 3
                    WHERE LabelCode IN ({string.Join(",", parameters)})
                    AND ContractNo = '{ContractNo}'
                    AND Contractor = '{Contractor}'
                    AND (Season + right(cast(Year as nvarchar(4)),2) = '{Season}'
                    OR Season + cast(Year as nvarchar(4)) = '{Season}')
                    ";

                    string connStr = ConfigurationManager
                        .ConnectionStrings["MyDbConnectionPro"]
                        .ConnectionString;
                    using (SqlConnection conn = new SqlConnection(connStr))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        for (int i = 0; i < LabelList.Count; i++)
                        {
                            cmd.Parameters.AddWithValue($"@p{i}", LabelList[i]);
                        }

                        conn.Open();

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }

                }
                else
                {
                    var parameters = POList
                        .Select((po, index) => $"@p{index}")
                        .ToList();

                    string sql = $@"
                    SELECT
                        PurchaseOrderNumber
                        ,CASE WHEN Season = 'M' THEN 'Summer '
                              WHEN Season = 'F' THEN 'Fall '
                              WHEN Season = 'S' THEN 'Spring '
                              WHEN Season = 'H' THEN 'Holiday '
                            END + cast(Year as nvarchar(4)) Season
                        ,ContractNo
                        ,CustomerStyle
                        ,FORMAT(ShipDate, 'MMM dd, yyyy', 'en-US') AS ShipDateText
                        ,ColorCode
                        ,LabelCode
                        ,Account
                        ,Contractor
                        ,Length
                        ,Width
                        ,Height
                        ,NetWeight
                        ,GrossWeight
                        ,QuantityPerCarton   
                    FROM ItemStyle i
                    LEFT JOIN 
                    (
                        SELECT 
	                            LSStyle,MAX(TRY_CAST(Length as float)) Length
	                            ,MAX(TRY_CAST(Width as float)) Width
	                            ,MAX(TRY_CAST(Height as float)) Height
	                            ,MAX(TRY_CAST(NetWeight as float)) NetWeight
	                            ,MAX(TRY_CAST(GrossWeight as float)) GrossWeight
                                ,MAX(TRY_CAST(QuantityPerCarton as float)) QuantityPerCarton
                        FROM PackingLine
                        WHERE IsDeleted = 0
                        GROUP BY LSStyle
                    )p ON p.LSStyle = i.LSStyle AND i.IsDeleted = 0 AND ISNULL(ItemStyleStatusCode,1) <> 3
                    WHERE PurchaseOrderNumber IN ({string.Join(",", parameters)})
                    ";

                    string connStr = ConfigurationManager
                        .ConnectionStrings["MyDbConnectionPro"]
                        .ConnectionString;
                    using (SqlConnection conn = new SqlConnection(connStr))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        for (int i = 0; i < POList.Count; i++)
                        {
                            cmd.Parameters.AddWithValue($"@p{i}", POList[i]);
                        }

                        conn.Open();

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                dataGridView1.DataSource = dt;
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu!");
                    return;
                }
                // Ví dụ: Click Create
                var type = txtType.Text.ToUpper();
                if (type != "HBNYO" && type != "HBE")
                {
                    MessageBox.Show("Type không hợp lệ!");
                    return;
                }
                driver.Navigate().GoToUrl("https://photo8.haddad.com/Create");
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));
                var typebutton = wait.Until(d =>
                    d.FindElement(By.XPath("//button[contains(., '" + type + "')]"))
                );
                typebutton.Click();


                // === AUTOMATION TASK ===
                FillCreateRequest(dt);

                CheckProcess = 2;
                lblStatus.Text = "Status: Waiting...";
                MessageBox.Show("Tasks Done!");
            }
            catch (Exception ex)
            {
                CheckProcess = 0;
                MessageBox.Show(ex.Message);
                driver.Quit();
                driver = null;
            }
        }

        private void FillCreateRequest(DataTable dt)
        {
            // Season
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));
            lblStatus.Text = "Status: Fill Draft...";

            var Season = dt.AsEnumerable()
                 .Select(r => r["Season"].ToString())
                 .FirstOrDefault();

            var dropdown = wait.Until(d =>
                d.FindElement(By.XPath("//div[contains(@class,'rw-dropdown-list-input')]"))
            );
            dropdown.Click();
            var input = wait.Until(d =>
                d.FindElement(By.XPath("//div[contains(@class,'rw-input')]//input[not(@tabindex='-1')]"))
            );
            wait.Until(ExpectedConditions.ElementToBeClickable(input));
            input.SendKeys(Season);
            input.SendKeys(OpenQA.Selenium.Keys.Enter);


            // Factory
            lblStatus.Text = "Status: Fill Factory...";

            var Factory = dt.AsEnumerable()
                 .Select(r => r["Contractor"].ToString())
                 .Distinct()
                 .First();
            input = driver.FindElement(By.CssSelector(".searchable-dropdown input"));
            input.Clear();
            input.SendKeys(Factory);
            input.SendKeys(OpenQA.Selenium.Keys.Enter);


            // Reference Number
            lblStatus.Text = "Status: Fill Reference Number...";

            var refInput = driver.FindElement(By.XPath("//div[@class='field-label' and text()='Reference Number']/following::input[1]"));
            var refText = dt.AsEnumerable()
                 .Select(r => r["ContractNo"].ToString())
                 .FirstOrDefault();
            refInput.Click();
            refInput.Clear();
                

            refInput.SendKeys(refText);
            refInput.SendKeys(OpenQA.Selenium.Keys.Enter);


            // Color
            lblStatus.Text = "Status: Fill Color...";

            List<string> colors = dt.AsEnumerable()
                 .Select(r => r["ColorCode"].ToString())
                 .Distinct()
                 .ToList();


            foreach (var color in colors)
            {
                // Focus vào ô input color
                var Colorinput = wait.Until(d =>
                    d.FindElement(By.XPath(
                        "//div[@class='field-label' and normalize-space()='Colors']" +
                        "/following-sibling::div" +
                        "//div[contains(@class,'searchable-dropdown')]//input"
                    ))
                );

                Colorinput.Click();
                Colorinput.Clear();
                Colorinput.SendKeys(color);
                Colorinput.SendKeys(OpenQA.Selenium.Keys.Enter);

                // WAIT cho đến khi màu vừa nhập xuất hiện trong selected-items
                wait.Until(d =>
                {
                    var selected = d.FindElements(
                        By.XPath("//div[contains(@class,'selected-items')]//div[contains(@class,'selected-item')]//div[@class='value']")
                    );

                    return selected.Any(s => s.Text.Trim().Equals(color, StringComparison.OrdinalIgnoreCase));
                });

            }


            // Styles
            lblStatus.Text = "Status: Fill Styles...";

            List<string> Styles = dt.AsEnumerable()
                 .Select(r => r["CustomerStyle"].ToString())
                 .Distinct()
                 .ToList();
            var Styleinput = wait.Until(d =>
                    d.FindElement(By.XPath(
                        "//div[@class='field-label' and normalize-space()='Styles']" +
                        "/following-sibling::div" +
                        "//div[contains(@class,'searchable-dropdown')]//input"
                    ))
                );

            foreach (var Style in Styles)
            {
                // Focus vào ô input Styleinput
                Styleinput = wait.Until(d =>
                    d.FindElement(By.XPath(
                        "//div[@class='field-label' and normalize-space()='Styles']" +
                        "/following-sibling::div" +
                        "//div[contains(@class,'searchable-dropdown')]//input"
                    ))
                );
                Styleinput.Click();
                Styleinput.SendKeys(Style);
                Styleinput.SendKeys(OpenQA.Selenium.Keys.Enter);

                // WAIT cho đến khi Styles vừa nhập xuất hiện trong selected-items
                wait.Until(d =>
                {
                    Styleinput.SendKeys(OpenQA.Selenium.Keys.Enter);
                    var selected = d.FindElements(
                        By.XPath("//div[contains(@class,'selected-items')]//div[contains(@class,'selected-item')]//div[@class='value']")
                    );
                    return selected.Any(s => s.Text.Trim().Equals(Style, StringComparison.OrdinalIgnoreCase));
                });
            }


            // Labels
            lblStatus.Text = "Status: Fill Labels...";

            List<string> Labels = dt.AsEnumerable()
                 .Select(r => r["LabelCode"].ToString())
                 .Distinct()
                 .ToList();



            foreach (var label in Labels)
            {
                // Focus vào ô input color
                var Labelinput = wait.Until(d =>
                    d.FindElement(By.XPath(
                        "//div[@class='field-label' and normalize-space()='Labels']" +
                        "/following-sibling::div" +
                        "//div[contains(@class,'searchable-dropdown')]//input"
                    ))
                );

                Labelinput.Click();
                Labelinput.SendKeys(label);
                Labelinput.SendKeys(OpenQA.Selenium.Keys.Enter);

                // WAIT cho đến khi label vừa nhập xuất hiện trong selected-items
                wait.Until(d =>
                {
                    var selected = d.FindElements(
                        By.XPath("//div[contains(@class,'selected-items')]//div[contains(@class,'selected-item')]//div[@class='value']")
                    );

                    return selected.Any(s => s.Text.Trim().Equals(label, StringComparison.OrdinalIgnoreCase));
                });

            }

            // ShipDate
            lblStatus.Text = "Status: Fill ShipDate...";

            var shipdate = dt.AsEnumerable()
                 .Select(r => r["ShipDateText"].ToString())
                 .Distinct()
                 .ToList();
            int i = 1;
            foreach (var ship in shipdate)
            {

                var shipDate1 = wait.Until(d =>
                d.FindElement(By.XPath("//div[@class='field-label' and normalize-space()='Ship Dates']/following::input["+i+"]"))
                );
                shipDate1.Click();
                shipDate1.SendKeys(ship);
                shipDate1.SendKeys(OpenQA.Selenium.Keys.Enter);
                Styleinput.Click();
                i++;
            }

            // Customer
            lblStatus.Text = "Status: Fill Customer...";

            var customerInput = wait.Until(d =>
                d.FindElement(By.XPath("//div[@class='field-label' and normalize-space()='Customer']/following::div[contains(@class,'searchable-dropdown')]//input"))
            );

            customerInput.Click();
            customerInput.Clear();

            string customerName = dt.AsEnumerable()
                 .Select(r => r["Account"].ToString())
                 .FirstOrDefault();

            customerInput.SendKeys(customerName);
            customerInput.SendKeys(OpenQA.Selenium.Keys.Enter);



            var firstItem = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath("(//div[contains(@class,'list')]//div[contains(@class,'item')])[1]")));
            firstItem.Click();


            //Select POs
            lblStatus.Text = "Status: Select POs...";

            List<string> poList = dt.AsEnumerable()
                 .Select(r => r["PurchaseOrderNumber"].ToString())
                 .Distinct()
                 .ToList();
            wait.Until(d => d.FindElements(By.CssSelector(".checkbox-list input[type='checkbox']")).Count > 0);

            foreach (var po in poList)
            {
                try
                {
                    var checkbox = driver.FindElement(By.Id(po));

                    if (!checkbox.Selected)
                    {
                        ((IJavaScriptExecutor)driver)
                            .ExecuteScript("arguments[0].click();", checkbox);
                    }
                }
                finally
                {
                    
                }
            }
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (driver != null)
            {
                driver.Quit();
                driver = null;
            }
        }

        private void SaveBtn(object sender, EventArgs e)
        {
            try
            {
                if (CheckProcess == 1)
                {
                    MessageBox.Show("Need Load Draft before!");
                }
                else if (CheckProcess == 2)
                {
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));
                    // Save Draft
                    var saveDraftBtn = wait.Until(d =>
                        d.FindElement(By.XPath(
                            "//div[contains(@class,'default-header')]//button[normalize-space()='Save Draft']"))
                    );
                    saveDraftBtn.Click();

                    bool isError = false;

                    wait.Until(d =>
                    {
                        // 1️⃣ CHECK URL SUCCESS
                        if (d.Url.Contains("/Details/GeneralInfo"))
                        {
                            return true;
                        }

                        // 2️⃣ CHECK ERROR MESSAGE
                        var error = d.FindElements(By.XPath(
                            "//div[contains(@class,'message-modal') and contains(.,' was not saved')]"
                        ));

                        if (error.Any())
                        {
                            isError = true;
                            return true;
                        }

                        return false;
                    });

                    if (isError)
                    {
                        MessageBox.Show("Save Draft failed! Please check again your information.");
                        return;
                    }

                    // Upload Images
                    string currentUrl = driver.Url;
                    while (!currentUrl.Contains("Details"))
                    {
                        currentUrl = driver.Url;
                    }

                    string baseUrl = Regex.Match(currentUrl, @"^.+?/Details").Value;
                    //Tag
                    var Tags = new List<string>
                    {
                        "Components",
                        "Packing",
                        "Garment",
                        "ClosedCarton"
                    };
                    foreach (var tag in Tags)
                    {
                        string tagUrl = baseUrl + $"/{tag}";

                        driver.Navigate().GoToUrl(tagUrl);
                        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));
                        var groupNames = wait.Until(d =>
                        {
                            var els = d.FindElements(By.XPath("//div[@class='group-name']"));
                            return els.Count > 0 ? els : null;
                        });

                        List<string> groupNameTexts = groupNames
                            .Select(g => g.Text.Trim())
                            .Where(t => !string.IsNullOrEmpty(t))
                            .ToList();

                        foreach (var groupName in groupNameTexts)
                        {
                            var files = Directory.GetFiles(Input_link.Text)
                                .Where(f => Path.GetFileName(f)
                                .StartsWith(groupName.Trim(), StringComparison.OrdinalIgnoreCase))
                                .ToArray();
                            if (files.Length == 0)
                                continue;
                            var attachments = wait.Until(d =>
                                    d.FindElement(By.XPath(
                                        $"//div[@class='group']" +
                                        $"[.//div[@class='group-name' and " +
                                        $"translate(normalize-space(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '{groupName.ToLower()}']]" +
                                        $"//div[@class='attachments']"
                                    ))
                                );
                            var tiles = attachments.FindElement(
                                By.XPath("./div[@class='tiles']")
                            );
                            ((IJavaScriptExecutor)driver)
                                .ExecuteScript("arguments[0].scrollIntoView({block:'center'});", tiles);

                            ((IJavaScriptExecutor)driver)
                                .ExecuteScript("arguments[0].click();", tiles);


                            var modal = wait.Until(d =>
                            {
                                var ms = d.FindElements(By.XPath("//div[contains(@class,'upload-file-modal')]"));
                                return ms.FirstOrDefault(x => x.Displayed);
                            });

                            var fileInput = wait.Until(d =>
                                modal.FindElement(By.XPath(".//input[@type='file']"))
                            );

                            ((IJavaScriptExecutor)driver)
                                .ExecuteScript("arguments[0].style.display='block'; arguments[0].value='';", fileInput);

                            fileInput.SendKeys(string.Join("\n", files));

                            var saveBtn = wait.Until(
                                ExpectedConditions.ElementToBeClickable(
                                    By.XPath("//div[contains(@class,'upload-file-modal')]//button[normalize-space()='Save']")
                                )
                            );

                            ((IJavaScriptExecutor)driver)
                                .ExecuteScript("arguments[0].click();", saveBtn);
                            wait.Until(d =>
                            {
                                var ms = d.FindElements(By.XPath("//div[contains(@class,'upload-file-modal')]"));
                                return ms.Count == 0 || ms.All(x => !x.Displayed);
                            });
                            wait.Until(d =>
                            {
                                var curtains = d.FindElements(By.CssSelector("div.curtain"));
                                return curtains.Count == 0 || curtains.All(c => !c.Displayed);
                            });
                            wait.Until(d =>
                            {
                                return ((IJavaScriptExecutor)d)
                                    .ExecuteScript("return document.readyState")
                                    .ToString() == "complete";
                            });
                        }
                        // Save Draft
                        saveDraftBtn = wait.Until(
                            ExpectedConditions.ElementToBeClickable(
                                By.XPath("//button[normalize-space()='Save Draft']")
                            )
                        );
                        // Scroll + click
                        ((IJavaScriptExecutor)driver)
                            .ExecuteScript("arguments[0].scrollIntoView({block:'center'});", saveDraftBtn);
                        Thread.Sleep(2000);
                        // Fill ClosedCarton data
                        if (tag == "ClosedCarton")
                        {
                            List<CartonData> cartons = new List<CartonData>();

                            foreach (DataRow row in dt.Rows)
                            {
                                if (row["PurchaseOrderNumber"] == DBNull.Value)
                                    continue;

                                var carton = new CartonData
                                {
                                    PO = row["PurchaseOrderNumber"].ToString().Trim(),
                                    Length = row["Length"]?.ToString(),
                                    Width = row["Width"]?.ToString(),
                                    Height = row["Height"]?.ToString(),
                                    NetWeight = row["NetWeight"]?.ToString(),
                                    GrossWeight = row["GrossWeight"]?.ToString()
                                };

                                cartons.Add(carton);
                            }
                            foreach (var data in cartons)
                            {
                                var carton = TryWaitCarton(driver, data.PO);
                                if (carton == null)
                                {
                                    Console.WriteLine($"⚠️ PO {data.PO} not found → skipped");
                                    return;
                                }

                                void Fill(string label, string value)
                                {
                                    if (string.IsNullOrWhiteSpace(value)) return;

                                    try
                                    {
                                        var input = carton.FindElement(By.XPath(
                                            $".//div[@class='field'][.//div[text()='{label}']]//input"
                                        ));

                                        ((IJavaScriptExecutor)driver)
                                            .ExecuteScript("arguments[0].value = '';", input);

                                        input.SendKeys(value);
                                    }
                                    catch (NoSuchElementException)
                                    {
                                        Console.WriteLine($"❌ Field '{label}' not found for PO {data.PO}");
                                    }
                                }

                                Fill("Length", data.Length);
                                Fill("Width", data.Width);
                                Fill("Height", data.Height);
                                Fill("Net Weight", data.NetWeight);
                                Fill("Gross Weight", data.GrossWeight);
                            }
                            int maxQty = dt.AsEnumerable()
                            .Where(r => r["QuantityPerCarton"] != DBNull.Value)
                            .Max(r => Convert.ToInt32(r["QuantityPerCarton"]));
                            try
                            {
                                var qtyInput = wait.Until(d =>
                                    d.FindElement(By.XPath("//div[contains(@class,'qtyPerCarton')]//input"))
                                );

                                ((IJavaScriptExecutor)driver)
                                    .ExecuteScript("arguments[0].value = '';", qtyInput);

                                qtyInput.SendKeys(maxQty.ToString());
                            }
                            catch (NoSuchElementException)
                            {
                                Console.WriteLine($"❌ Field 'QuantityPerCarton' not found ");
                            }
                        }
                        saveDraftBtn.Click();
                    }
                }
                else
                {
                    MessageBox.Show("Need Login and Load Draft before!");
                }
            }
            finally
            { }
        }

        private void Input_link_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Image = Input_link.Text;
            Properties.Settings.Default.Save();
        }

        private void txtType_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Type = txtType.Text;
            Properties.Settings.Default.Save();
        }

        private void txtSeason_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Season = txtSeason.Text;
            Properties.Settings.Default.Save();
        }

        private void txtContractor_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Contractor = txtContractor.Text;
            Properties.Settings.Default.Save();
        }

        private IWebElement TryWaitCarton(IWebDriver driver, string po)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                return wait.Until(d =>
                    d.FindElement(By.XPath(
                        $"//div[@class='carton'][.//div[@class='po-number' and normalize-space()='{po}']]"
                    ))
                );
            }
            catch (WebDriverTimeoutException)
            {
                return null;
            }
        }
        public class CartonData
        {
            public string PO { get; set; }
            public string Length { get; set; }
            public string Width { get; set; }
            public string Height { get; set; }
            public string NetWeight { get; set; }
            public string GrossWeight { get; set; }
        }
        private void UploadDraftImage(DataTable dt)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));
            
            if (CheckProcess == 1)
            {
                MessageBox.Show("Need Load Draft before!");
            }
            else if (CheckProcess == 2)
            {
                //WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));
                // Save Draft
                lblStatus.Text = "Status: Save Draft...";

                var saveDraftBtn = wait.Until(d =>
                    d.FindElement(By.XPath(
                        "//div[contains(@class,'default-header')]//button[normalize-space()='Save Draft']"))
                );
                saveDraftBtn.Click();

                bool isError = false;

                wait.Until(d =>
                {
                    // 1️⃣ CHECK URL SUCCESS
                    if (d.Url.Contains("/Details/GeneralInfo"))
                    {
                        return true;
                    }

                    // 2️⃣ CHECK ERROR MESSAGE
                    var error = d.FindElements(By.XPath(
                        "//div[contains(@class,'message-modal') and contains(.,' was not saved')]"
                    ));

                    if (error.Any())
                    {
                        isError = true;
                        return true;
                    }

                    return false;
                });

                if (isError)
                {
                    MessageBox.Show("Save Draft failed! Please check again your information.");
                    return;
                }

                // Upload Images
                lblStatus.Text = "Status: Save Draft...";

                string currentUrl = driver.Url;
                while (!currentUrl.Contains("Details"))
                {
                    currentUrl = driver.Url;
                }

                string baseUrl = Regex.Match(currentUrl, @"^.+?/Details").Value;
                //Tag
                var Tags = new List<string>
                    {
                        "Components",
                        "Garment",
                        "Packing",
                        "ClosedCarton"
                    };
                foreach (var tag in Tags)
                {
                    lblStatus.Text = $"Status: Upload Images for {tag}...";

                    string tagUrl = baseUrl + $"/{tag}";

                    driver.Navigate().GoToUrl(tagUrl);
                    wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));
                    var groupNames = wait.Until(d =>
                    {
                        var els = d.FindElements(By.XPath("//div[@class='group-name']"));
                        return els.Count > 0 ? els : null;
                    });

                    List<string> groupNameTexts = groupNames
                        .Select(g => g.Text.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();

                    foreach (var groupName in groupNameTexts)
                    {
                        var files = Directory.GetFiles(Input_link.Text)
                            .Where(f => Path.GetFileName(f)
                            .StartsWith(groupName.Trim(), StringComparison.OrdinalIgnoreCase))
                            .ToArray();
                        if (files.Length == 0)
                            continue;
                        var attachments = wait.Until(d =>
                                d.FindElement(By.XPath(
                                    $"//div[@class='group']" +
                                    $"[.//div[@class='group-name' and " +
                                    $"translate(normalize-space(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '{groupName.ToLower()}']]" +
                                    $"//div[@class='attachments']"
                                ))
                            );
                        var tiles = attachments.FindElement(
                            By.XPath("./div[@class='tiles']")
                        );
                        ((IJavaScriptExecutor)driver)
                            .ExecuteScript("arguments[0].scrollIntoView({block:'center'});", tiles);

                        ((IJavaScriptExecutor)driver)
                            .ExecuteScript("arguments[0].click();", tiles);


                        var modal = wait.Until(d =>
                        {
                            var ms = d.FindElements(By.XPath("//div[contains(@class,'upload-file-modal')]"));
                            return ms.FirstOrDefault(x => x.Displayed);
                        });

                        var fileInput = wait.Until(d =>
                            modal.FindElement(By.XPath(".//input[@type='file']"))
                        );

                        ((IJavaScriptExecutor)driver)
                            .ExecuteScript("arguments[0].style.display='block'; arguments[0].value='';", fileInput);

                        fileInput.SendKeys(string.Join("\n", files));

                        var saveBtn = wait.Until(
                            ExpectedConditions.ElementToBeClickable(
                                By.XPath("//div[contains(@class,'upload-file-modal')]//button[normalize-space()='Save']")
                            )
                        );

                        ((IJavaScriptExecutor)driver)
                            .ExecuteScript("arguments[0].click();", saveBtn);
                        wait.Until(d =>
                        {
                            var ms = d.FindElements(By.XPath("//div[contains(@class,'upload-file-modal')]"));
                            return ms.Count == 0 || ms.All(x => !x.Displayed);
                        });
                        wait.Until(d =>
                        {
                            var curtains = d.FindElements(By.CssSelector("div.curtain"));
                            return curtains.Count == 0 || curtains.All(c => !c.Displayed);
                        });
                        wait.Until(d =>
                        {
                            return ((IJavaScriptExecutor)d)
                                .ExecuteScript("return document.readyState")
                                .ToString() == "complete";
                        });
                    }
                    // Save Draft
                    saveDraftBtn = wait.Until(
                        ExpectedConditions.ElementToBeClickable(
                            By.XPath("//button[normalize-space()='Save Draft']")
                        )
                    );
                    // Scroll + click
                    ((IJavaScriptExecutor)driver)
                        .ExecuteScript("arguments[0].scrollIntoView({block:'center'});", saveDraftBtn);
                    Thread.Sleep(2000);
                    // Fill ClosedCarton data
                    if (tag == "ClosedCarton")
                    {
                        List<CartonData> cartons = new List<CartonData>();

                        foreach (DataRow row in dt.Rows)
                        {
                            if (row["PurchaseOrderNumber"] == DBNull.Value)
                                continue;

                            var carton = new CartonData
                            {
                                PO = row["PurchaseOrderNumber"].ToString().Trim(),
                                Length = row["Length"]?.ToString(),
                                Width = row["Width"]?.ToString(),
                                Height = row["Height"]?.ToString(),
                                NetWeight = row["NetWeight"]?.ToString(),
                                GrossWeight = row["GrossWeight"]?.ToString()
                            };

                            cartons.Add(carton);
                        }
                        foreach (var data in cartons)
                        {
                            var carton = TryWaitCarton(driver, data.PO);
                            if (carton == null)
                            {
                                Console.WriteLine($"⚠️ PO {data.PO} not found → skipped");
                                return;
                            }

                            void Fill(string label, string value)
                            {
                                if (string.IsNullOrWhiteSpace(value)) return;

                                try
                                {
                                    var input = carton.FindElement(By.XPath(
                                        $".//div[@class='field'][.//div[text()='{label}']]//input"
                                    ));

                                    ((IJavaScriptExecutor)driver)
                                        .ExecuteScript("arguments[0].value = '';", input);

                                    input.SendKeys(value);
                                }
                                catch (NoSuchElementException)
                                {
                                    Console.WriteLine($"❌ Field '{label}' not found for PO {data.PO}");
                                }
                            }

                            Fill("Length", data.Length);
                            Fill("Width", data.Width);
                            Fill("Height", data.Height);
                            Fill("Net Weight", data.NetWeight);
                            Fill("Gross Weight", data.GrossWeight);
                        }
                        int maxQty = dt.AsEnumerable()
                            .Where(r => r["QuantityPerCarton"] != DBNull.Value)
                            .Max(r => Convert.ToInt32(r["QuantityPerCarton"]));
                        try
                        {
                            var qtyInput = wait.Until(d =>
                                d.FindElement(By.XPath("//div[contains(@class,'qtyPerCarton')]//input"))
                            );

                            ((IJavaScriptExecutor)driver)
                                .ExecuteScript("arguments[0].value = '';", qtyInput);

                            qtyInput.SendKeys(maxQty.ToString());
                        }
                        catch (NoSuchElementException)
                        {
                            Console.WriteLine($"❌ Field 'QuantityPerCarton' not found ");
                        }
                        
                    }    
                        
                    saveDraftBtn.Click();
                }
                lblStatus.Text = $"Status: Done upload!";
            }
            else
            {
                MessageBox.Show("Need Login and Load Draft before!");
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra đã login
                if (driver == null)
                {
                    MessageBox.Show("Bạn chưa đăng nhập!");
                    return;
                }
                lblStatus.Text = "Status: Prepare data...";

                string rawInputLb = rtbLabel.Text;
                string rawInputPO = rtbPO.Text;
                string ContractNo = txtContractNo.Text;
                string Contractor = txtContractor.Text;
                string Season = txtSeason.Text;

                // Cho phép ngăn cách bằng: , hoặc xuống dòng
                var LabelList = rawInputLb
                    .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();
                var POList = rawInputPO
                    .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();
                if ((LabelList.Count == 0 || string.IsNullOrWhiteSpace(ContractNo)
                    || string.IsNullOrWhiteSpace(Contractor)
                    || string.IsNullOrWhiteSpace(Season)
                    ) && POList.Count == 0)
                {
                    MessageBox.Show("Vui lòng nhập đủ Contractor-ContractNo-Season-Label hoặc nhập theo PO");
                    return;
                }


                if (POList.Count == 0)
                {
                    var parameters = LabelList
                        .Select((Label, index) => $"@p{index}")
                        .ToList();

                    string sql = $@"
                    SELECT
                        PurchaseOrderNumber
                        ,CASE WHEN Season = 'M' THEN 'Summer '
                              WHEN Season = 'F' THEN 'Fall '
                              WHEN Season = 'S' THEN 'Spring '
                              WHEN Season = 'H' THEN 'Holiday '
                            END + cast(Year as nvarchar(4)) Season
                        ,ContractNo
                        ,CustomerStyle
                        ,FORMAT(ShipDate, 'MMM dd, yyyy', 'en-US') AS ShipDateText
                        ,ColorCode
                        ,LabelCode
                        ,Account
                        ,Contractor
                        ,Length
                        ,Width
                        ,Height
                        ,NetWeight
                        ,GrossWeight
                        ,QuantityPerCarton
                    FROM ItemStyle i
                    LEFT JOIN 
                    (
                        SELECT 
	                        LSStyle,MAX(TRY_CAST(Length as float)) Length
	                        ,MAX(TRY_CAST(Width as float)) Width
	                        ,MAX(TRY_CAST(Height as float)) Height
	                        ,MAX(TRY_CAST(NetWeight as float)) NetWeight
	                        ,MAX(TRY_CAST(GrossWeight as float)) GrossWeight
                            ,MAX(TRY_CAST(QuantityPerCarton as float)) QuantityPerCarton
                        FROM PackingLine
                        WHERE IsDeleted = 0
                        GROUP BY LSStyle
                    )p ON p.LSStyle = i.LSStyle AND i.IsDeleted = 0 AND ISNULL(ItemStyleStatusCode,1) <> 3
                    WHERE LabelCode IN ({string.Join(",", parameters)})
                    AND ContractNo = '{ContractNo}'
                    AND Contractor = '{Contractor}'
                    AND (Season + right(cast(Year as nvarchar(4)),2) = '{Season}'
                    OR Season + cast(Year as nvarchar(4)) = '{Season}')
                    ";

                    string connStr = ConfigurationManager
                        .ConnectionStrings["MyDbConnectionPro"]
                        .ConnectionString;
                    using (SqlConnection conn = new SqlConnection(connStr))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        for (int i = 0; i < LabelList.Count; i++)
                        {
                            cmd.Parameters.AddWithValue($"@p{i}", LabelList[i]);
                        }

                        conn.Open();

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }

                }
                else
                {
                    var parameters = POList
                        .Select((po, index) => $"@p{index}")
                        .ToList();

                    string sql = $@"
                    SELECT
                        PurchaseOrderNumber
                        ,CASE WHEN Season = 'M' THEN 'Summer '
                              WHEN Season = 'F' THEN 'Fall '
                              WHEN Season = 'S' THEN 'Spring '
                              WHEN Season = 'H' THEN 'Holiday '
                            END + cast(Year as nvarchar(4)) Season
                        ,ContractNo
                        ,CustomerStyle
                        ,FORMAT(ShipDate, 'MMM dd, yyyy', 'en-US') AS ShipDateText
                        ,ColorCode
                        ,LabelCode
                        ,Account
                        ,Contractor
                        ,Length
                        ,Width
                        ,Height
                        ,NetWeight
                        ,GrossWeight
                        ,QuantityPerCarton
                    FROM ItemStyle i
                    LEFT JOIN 
                    (
                        SELECT 
	                            LSStyle,MAX(TRY_CAST(Length as float)) Length
	                            ,MAX(TRY_CAST(Width as float)) Width
	                            ,MAX(TRY_CAST(Height as float)) Height
	                            ,MAX(TRY_CAST(NetWeight as float)) NetWeight
	                            ,MAX(TRY_CAST(GrossWeight as float)) GrossWeight
                                ,MAX(TRY_CAST(QuantityPerCarton as float)) QuantityPerCarton
                        FROM PackingLine
                        WHERE IsDeleted = 0
                        GROUP BY LSStyle
                    )p ON p.LSStyle = i.LSStyle AND i.IsDeleted = 0 AND ISNULL(ItemStyleStatusCode,1) <> 3
                    WHERE PurchaseOrderNumber IN ({string.Join(",", parameters)})
                    ";

                    string connStr = ConfigurationManager
                        .ConnectionStrings["MyDbConnectionPro"]
                        .ConnectionString;
                    using (SqlConnection conn = new SqlConnection(connStr))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        for (int i = 0; i < POList.Count; i++)
                        {
                            cmd.Parameters.AddWithValue($"@p{i}", POList[i]);
                        }

                        conn.Open();

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                dataGridView1.DataSource = dt;
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu!");
                    return;
                }
                // Ví dụ: Click Create
                var type = txtType.Text.ToUpper();
                if (type != "HBNYO" && type != "HBE")
                {
                    MessageBox.Show("Type không hợp lệ!");
                    return;
                }
                lblStatus.Text = "Status: Load Draft...";

                driver.Navigate().GoToUrl("https://photo8.haddad.com/Create");
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));
                var typebutton = wait.Until(d =>
                    d.FindElement(By.XPath("//button[contains(., '" + type + "')]"))
                );
                typebutton.Click();
                // === AUTOMATION TASK ===
                FillCreateRequest(dt);
                CheckProcess = 2;

                UploadDraftImage(dt);
                MessageBox.Show("Task Done!");


            }
            catch (Exception ex)
            {
                CheckProcess = 0;
                MessageBox.Show(ex.Message);
                driver.Quit();
                driver = null;
            }
        }
        private void LongProcess()
        {
            Thread.Sleep(5000); // giả lập xử lý lâu
        }

        private async void button4_Click_1(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                LongProcess(); // chạy ngầm
            });

        }
    }
}

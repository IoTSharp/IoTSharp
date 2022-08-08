using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IoTSharp.EasyUse
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void 新建ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pg.SelectedObject=new  IoTSharp.AppSettings();
        }
        public string appsettings_file { get; set; }
        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "配置文件(appsettings.*.json)|appsettings.*.json|所有文件(*.*)|*.*";
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    var fi = new System.IO.FileInfo(openFileDialog.FileName);
                    appsettings_file = fi.FullName;
                    var appjson = new System.IO.FileInfo(System.IO.Path.Combine(fi.Directory.FullName, "appsettings.json"));
                    if (fi.Exists && appjson.Exists)
                    {
                        var jo = JObject.Parse(System.IO.File.ReadAllText(appjson.FullName));
                        var ja = JObject.Parse(System.IO.File.ReadAllText(fi.FullName));
                        jo.Merge(ja);
                        pg.SelectedObject = jo.ToObject<IoTSharp.AppSettings>();

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settings = pg.SelectedObject as AppSettings;
            var fi = new FileInfo(appsettings_file);
            var appjson = new System.IO.FileInfo(System.IO.Path.Combine(fi.Directory.FullName, "appsettings.json"));
            if (fi.Exists && appjson.Exists)
            {
                var jo = JObject.Parse(System.IO.File.ReadAllText(appjson.FullName));
                var ja = JObject.Parse(System.IO.File.ReadAllText(fi.FullName));
                string json2 = JsonConvert.SerializeObject(settings, new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                });
                System.IO.File.WriteAllText(fi.FullName, json2);
            }

        }

    }
}
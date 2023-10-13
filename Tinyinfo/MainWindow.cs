using System;
using System.Drawing;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using Hardware.Info;
using static System.Net.Mime.MediaTypeNames;
using IniParser;
using IniParser.Model;

namespace Tinyinfo
{
	public partial class MainWindow : Form
	{
		static readonly IHardwareInfo hardwareInfo = new HardwareInfo();
		private delegate void SafeCallDelegate(string text);
		public MainWindow()
		{
			InitializeComponent();

			//	Load Theme
			refreshTheme();
		}

		//	Thread for updating info in background
		Thread thread;

		//	Runs on form load
		public void startup(object sender, EventArgs e)
		{
			getdata(false);
		}

		//	TODO: Put CPU info in separate thread for improved speed

		//	collect system info and write to textBox1
		public void getdata(bool loop)
		{
			var nl = Environment.NewLine;
			do
			{
				//	Refresh lists
				hardwareInfo.RefreshCPUList(true);
				hardwareInfo.RefreshMemoryList();
				hardwareInfo.RefreshBIOSList();
				hardwareInfo.RefreshMotherboardList();
				hardwareInfo.RefreshVideoControllerList();
				hardwareInfo.RefreshBatteryList();

				foreach (var cpu in hardwareInfo.CpuList)
				{
					//	CPU Info
					WriteTextSafe("CPU:" + nl);

					//	CPU ID
					AppendTextSafe("\tID: " + cpu.ProcessorId + nl);

					//	Manufacturer and Model
					AppendTextSafe("\tManufacturer: " + cpu.Manufacturer + nl);
					AppendTextSafe("\tModel: " + cpu.Name.Replace("  ", "") + nl);

					// Description
					AppendTextSafe("\tDescription: " + cpu.Description + nl);

					//	Socket
					AppendTextSafe("\tSocket: " + cpu.SocketDesignation + nl);

					//	Cores and Threads
					AppendTextSafe("\tCore Amount: " + cpu.NumberOfCores + " Physical, " + cpu.NumberOfLogicalProcessors + " Logical" + nl);

					//	VM Firmware
					AppendTextSafe("\tVirtualization Firmware Enabled: " + cpu.VirtualizationFirmwareEnabled + nl);


					//	Clockspeeds
					AppendTextSafe("\tClockspeeds:" + nl);
					//	Current Clockspeed in mHz
					AppendTextSafe("\t\t" + cpu.CurrentClockSpeed + "mHz Current" + nl);
					//	Base Clockspeed in mHz
					AppendTextSafe("\t\t" + cpu.MaxClockSpeed + "mHz Base");

					//	Graphics
					AppendTextSafe(nl + "Video: ");
					//	Create GPU ID
					int id = 0;
					foreach (var gpu in hardwareInfo.VideoControllerList)
					{
						//	Write capacity into float and convert to GB
						float vmemsize = gpu.AdapterRAM;
						vmemsize /= 1073741824;

						//	GPU ID
						AppendTextSafe(nl + "\tGPU " + id + ":" + nl);

						//	Name
						AppendTextSafe("\t\tName: " + gpu.Name + nl);

						//	Manufacturer
						AppendTextSafe("\t\tManufacturer: " + gpu.Manufacturer + nl);

						//	Description
						AppendTextSafe("\t\tDescription: " + gpu.VideoProcessor + nl);

						//	Video mode
						AppendTextSafe("\t\tVideo Mode: " + gpu.VideoModeDescription + " x " + gpu.CurrentRefreshRate + "Hz x " + gpu.CurrentBitsPerPixel + " Bit" + nl);

						//	Video memory amount
						AppendTextSafe("\t\tVRAM Amount: " + vmemsize + "GB" + nl);

						//	Maximum Refresh rate
						AppendTextSafe("\t\tMaximum Refresh Rate: " + gpu.MaxRefreshRate + "Hz" + nl);

						//	Minimum Refresh rate
						AppendTextSafe("\t\tMinimum Refresh Rate: " + gpu.MinRefreshRate + "Hz " + nl);

						//	Driver
						AppendTextSafe("\t\tDriver: " + nl);

						//	Driver Version
						AppendTextSafe("\t\t\tVersion: " + gpu.DriverVersion + nl);

						//	Driver Date
						AppendTextSafe("\t\t\tDate: " + gpu.DriverDate + nl);

						//	Increment GPU ID
						id++;
					}

					//	Memory
					AppendTextSafe(nl + nl + "Memory:");
					foreach (var memory in hardwareInfo.MemoryList)
					{
						//	Write capacity into float and convert to GB
						float memsize = memory.Capacity;
						memsize /= 1073741824;

						//	Bank number
						AppendTextSafe(nl + "\t" + memory.BankLabel + ":" + nl);

						//	Manufacturer
						AppendTextSafe("\t\tManufacturer: " + memory.Manufacturer + nl);

						//	Size
						AppendTextSafe("\t\t\tSize: " + memsize + "GB" + nl);

						//	Speed
						AppendTextSafe("\t\t\tSpeed: " + memory.Speed + "mT/s" + nl);

						//	Part Number
						AppendTextSafe("\t\t\tPart No.: " + memory.PartNumber + nl);

						//	Form Factor
						AppendTextSafe("\t\t\tForm Factor: " + memory.FormFactor + nl);

						//	Minimum Voltage
						AppendTextSafe("\t\t\tMin. Voltage: " + memory.MinVoltage + "mV" + nl);

						//	Maximum voltage
						AppendTextSafe("\t\t\tMax. Voltage: " + memory.MaxVoltage + "mV");
					}

					//	Motherboard
					foreach (var motherboard in hardwareInfo.MotherboardList)
					{
						AppendTextSafe(nl + "Motherboard: " + nl);
						//	Manufacturer
						AppendTextSafe("\tManufacturer: " + motherboard.Manufacturer + nl);
						//	Model
						AppendTextSafe("\tModel: " + motherboard.Product + nl);
						//	Serial Number
						AppendTextSafe("\tSerial No.: " + motherboard.SerialNumber);
					}

					//	BIOS Info
					foreach (var bios in hardwareInfo.BiosList)
					{
						AppendTextSafe(nl + "BIOS: " + nl);
						//	Manufacturer
						AppendTextSafe("\tManufacturer: " + bios.Manufacturer + nl);
						//	Name
						AppendTextSafe("\tName: " + bios.Name + nl);
						//	Version
						AppendTextSafe("\tVersion: " + bios.Version + nl);
						//	Release Date
						AppendTextSafe("\tRelease Date: " + bios.ReleaseDate);
					}

					//	Battery Info
					foreach (var battery in hardwareInfo.BatteryList)
					{
						AppendTextSafe(nl + "Battery: " + nl);
						//	Status
						AppendTextSafe("\tStatus: " + battery.BatteryStatus + nl);
						//	Status Description
						AppendTextSafe("\tStatus Description: " + battery.BatteryStatusDescription + nl);
						//	Battery Percentage
						AppendTextSafe("\tBattery Percentage: " + battery.EstimatedChargeRemaining + "%" + nl);
						//	Expected Life
						AppendTextSafe("\tExpected Life: " + battery.ExpectedLife + nl);
						//	Time to Charge
						AppendTextSafe("\tTime until fully charged: " + battery.TimeToFullCharge + nl);
						//	Time on Battery
						AppendTextSafe("\tTime on Battery: " + battery.TimeOnBattery + nl);
						//	Capacities
						AppendTextSafe("\tCapacities: " + nl);
						//	Design Capacity
						AppendTextSafe("\t\tDesign Capacity: " + battery.DesignCapacity + nl);
						//	Current Capaity
						AppendTextSafe("\t\tFull Charge Capacity: " + battery.FullChargeCapacity + nl);
					}
				}

				ShowInfo("");

			} while (loop);
		}

		// Safely Overwrite on textbox content
		private void ShowInfo(string text)
		{
            if (textBox1.InvokeRequired)
            {
                var d = new SafeCallDelegate(ShowInfo);
                textBox1.Invoke(d,new object[] { InfoTextBuffer });
            }
            else
            {
                textBox1.Text = InfoTextBuffer;
            }
        }

        // Creating String To Push it later on textbox
        private string InfoTextBuffer = "";
		private void WriteTextSafe(string text)
		{
            // NOTE (HOUDAIFA) : Faster Way

            InfoTextBuffer = text;


            return;
		}

		// Appand Text To Text Buffer
		private void AppendTextSafe(string text)
		{
            // NOTE (HOUDAIFA) : Faster Way

			InfoTextBuffer += text;

            return;
		}

		//	Starts thread, changes button states, update info text and increments progress bar
		public void loadInfo()
		{
			label1.Visible = true;
			progressBar1.Visible = true;
			button1.Enabled = false;
			label1.Text = "Loading System Info.";
			progressBar1.Value = 25;
			hardwareInfo.RefreshCPUList();
			button2.Enabled = true;
			label1.Text = "Loading System Info..";
			progressBar1.Value = 50;
			hardwareInfo.RefreshOperatingSystem();
			button3.Enabled = true;
			label1.Text = "Loading System Info...";
			progressBar1.Value = 75;
			thread = new Thread(() => getdata(true));
			progressBar1.Value = 85;
			thread.IsBackground = true;
			label1.Text = "Loading System Info....";
			progressBar1.Value = 100;
			label1.Visible = false;
			thread.Start();
			progressBar1.Visible = false;
		}

		//	Stop update thread
		public void stopUpdate()
		{
			button3.Enabled = false;
			thread.Abort();
			button2.Enabled = false;
			button1.Enabled = true;
		}

		//	Load System info when Start Button is pressed
		public void button1_Click(object sender, EventArgs e)
		{
			loadInfo();
		}

		//	Change Button state and abort thread when Stop Button is pressed
		private void button2_Click(object sender, EventArgs e)
		{
			stopUpdate();
		}

		//	Start/Stop thread when Play/Pause button is pressed. not used as of v1.4
		private void button3_Click(object sender, EventArgs e)
		{
			if (thread.ThreadState == System.Threading.ThreadState.Stopped) {
				thread.Start();
			}
			else
			{
				thread.Abort();
			}
		}

		private void onTopCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			if (onTopCheckbox.Checked)
			{
				ActiveForm.TopMost = true;
			}
			else
			{
				ActiveForm.TopMost = false;
			}
		}

		public void refreshTheme()
		{
			//	Check if file exists, if it doesnt create it with default settings
			if (File.Exists("./tinyinfo.ini") == false)
			{
				File.WriteAllText("./tinyinfo.ini", "[tinyinfo]\ntheme=light\nfont=10");
			}

			//	Create ini parser and read ini file
			var parser = new FileIniDataParser();
			IniData data = parser.ReadFile("./tinyinfo.ini");

			//	Read Settings
			//	Set theme
			if (data.GetKey("tinyinfo.theme") == "dark")
			{
				//	Dark theme
				ForeColor = Color.White;
				BackColor = Color.Black;
				button1.ForeColor = Color.Black;
				button2.ForeColor = Color.Black;
				button3.ForeColor = Color.Black;
				onTopCheckbox.ForeColor = Color.Black;
				onTopCheckbox.BackColor = Color.Gray;
				panel1.BackColor = Color.FromName("ButtonFace");
				panel1.ForeColor = Color.White;
				textBox1.BackColor = Color.Black;
				textBox1.ForeColor = Color.White;
			}
			else
			{
				//	Light theme
				ForeColor = Color.Black;
				BackColor = Color.White;
				button1.ForeColor = Color.Black;
				button2.ForeColor = Color.Black;
				button3.ForeColor = Color.Black;
				onTopCheckbox.ForeColor = Color.Black;
				onTopCheckbox.BackColor = Color.White;
				panel1.BackColor = Color.White;
				panel1.ForeColor = Color.Black;
				textBox1.BackColor = Color.White;
				textBox1.ForeColor = Color.Black;
			}

			//	Set font size
			var font = new Font("Segoe UI", Convert.ToInt32(data.GetKey("tinyinfo.font")));

			textBox1.Font = font;
		}

		//	Opens Settings Window
		private void settings_Click(object sender, EventArgs e)
		{
			//	Create Settings Window
			var settings = new SettingsWindow();
			settings.ShowDialog();
			//	Reload Theme
			refreshTheme();
		}

		//	Create ShellAbout
		[DllImport("shell32.dll")]
		static extern int ShellAbout(IntPtr hwnd, string szApp, string szOtherStuff, IntPtr hIcon);

		//	Opens ShellAbout Dialog to display version info
		private void about_Click(object sender, EventArgs e)
		{
			//	Write version to string
			string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			
			//	Create ShellAbout dialog
			ShellAbout(IntPtr.Zero, "Tinyinfo " + version, "Tinyinfo v." + version, Icon.Handle);
		}

		//	Opens GitHub repo in browser
		private void github_Click(object sender, EventArgs e)
		{
			Process.Start("https://github.com/Lion-Craft/Tinyinfo");
		}

		private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			stopUpdate();
		}
	}
}

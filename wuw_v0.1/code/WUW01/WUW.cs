//*****************************************************************************************
//  WUW Wear Ur World
//  Pranav Mistry (pranav@mit.edu)
//  David Chang (dchang@mit.edu)
//
//  MIT Media Lab 2008
//  Fluid Interfaces
//
//  WUW.cs
//  Version 7. NY Integrate.
//  Added 4 at a time
//  Added gray screens between seconds
//  Added marker tolerance save
//  Made Test Bolder.
//*****************************************************************************************


using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices; 
using System.Text;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Media.Imaging;

using TouchlessLib;

//added for ny
//liyan chang
//TAGGING
using System.Threading;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using NyARToolkitCSUtils.Capture;
using NyARToolkitCSUtils.Direct3d;
using NyARToolkitCSUtils.NyAR;
using jp.nyatla.nyartoolkit.cs;
using jp.nyatla.nyartoolkit.cs.core;
using jp.nyatla.nyartoolkit.cs.detector;
//END TAGGING

namespace WUW01
{   
    public partial class WUW : Form
    {

		#region Global Variables
        
        //TAGGING
        //NYAR
        //initialize nyar stuff.
        private const int SCREEN_WIDTH = 320;
        private const int SCREEN_HEIGHT = 240;
        private const String AR_CODE_FILE1 = "patt.sample1";
        private const String AR_CODE_FILE2 = "patt.sample2";
        private const String AR_CODE_FILE3 = "patt.hiro";
        private const String AR_CAMERA_FILE = "camera_para.dat";
        private NyARSingleDetectMarker _ar1;
        private NyARSingleDetectMarker _ar2;
        private NyARSingleDetectMarker _ar3;
        private NyARSingleDetectMarker _arFinal;
        private DsBGRX32Raster _raster;
        bool is_marker_enable;

        private Microsoft.DirectX.Matrix __MainLoop_trans_matrix = new Microsoft.DirectX.Matrix();
        private NyARTransMatResult __MainLoop_nyar_transmat = new NyARTransMatResult();
        private NyARD3dUtil _utils;
        //END TAGGING

        //Touchless
        private TouchlessMgr _touchlessMgr;
        private static DateTime _dtFrameLast;
        private static DateTime _latestFrameTime; //Liyanchang
        private static int _nFrameCount;
        private static Point _markerCenter;
        private static float _markerRadius;
        private static Marker _markerSelected;
        private static bool _fAddingMarker;
        private static int _addedMarkerCount;        
        private static bool _fUpdatingMarkerUI;
        private static Image _latestFrame;
        private static bool _latestFrameTimeSegment; //liyanchang
        private static bool _drawSelectionAdornment;        
        private static int _ratioScreenCameraHeight;
        private static int _ratioScreenCameraWidth;

        //Gesture 
        private GeometricRecognizer	_rec;
		private bool				_recording;
		private bool				_isDown;
		private ArrayList			_points;
		private ViewForm			_viewFrm;        

        //InkCanvas
        System.Windows.Controls.InkCanvas inkCanvas1;
        System.Windows.Controls.InkCanvas inkCanvas2;

        //Clock Control
        WPFControl_Clock01.UserControl1 Control_clock = new WPFControl_Clock01.UserControl1();

        //Weather Control
        WPFControl_Weather01.UserControl1 Control_weather = new WPFControl_Weather01.UserControl1();

        //Album Control
        //WPFControl_Album02.UserControl1 Control_album = new WPFControl_Album02.UserControl1();

        //Album Control
        //WPFControl_Menu02.UserControl1 Control_menu = new WPFControl_Menu02.UserControl1();        
        
        //WUW
        Marker m, n, o, p;

        //Booleans to signify application statuses.
        private bool gestureDemo = true; //this is always true
        private bool menuDemo = false;
        private bool drawDemo = false;
        private bool mapDemo = false;        
        private bool photoDemo = false;
        private bool clockDemo = false;
        private bool weatherDemo = false;
        private bool newspaperDemo = false;
        private bool mailDemo = false;
        private bool jeffDemo = false; //liyanchang
        private bool flyDemo = false; //liyanchang
        private bool threedDemo = false; //liyanchang
        
        //Menu Timer for handSign_Menu()
        double? menuStart = null;
        double photoTaken = 0.0;

        //Photo Timer for PhotoDemo
        System.Windows.Forms.Timer Timer;
        int timerSum = 0;

        //Toggles for various functions
        private bool _show_settings = false;
        private bool _mousedown = false;
        private bool _zoomtoggle = false;
        private bool _mapmoderoad = true;

		#endregion Global Variables

        #region Mouse Interaction
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        private const int MOUSEEVENTF_WHEEL = 0x800;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x20;
        private const int MOUSEEVENTF_MIDDLEUP = 0x40;

        #endregion Mouse Interaction

        #region Environment

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (_touchlessMgr.MarkerCount >= 4)
            {
                m.OnChange -= new EventHandler<MarkerEventArgs>(m_OnChange);
                n.OnChange -= new EventHandler<MarkerEventArgs>(n_OnChange);
                o.OnChange -= new EventHandler<MarkerEventArgs>(o_OnChange);
                p.OnChange -= new EventHandler<MarkerEventArgs>(p_OnChange);
                m = null;
                n = null;
                o = null;
                p = null;
            }

            Environment.Exit(0);
        }
        private void btnShowHide_Hover(object sener, EventArgs e)
        {
            if (_show_settings)
            {
                tabSettings.Hide();
                pictureBoxDisplay.Hide();
                btnExit.Hide();
                _show_settings = false;
            }
            else
            {
                tabSettings.Show();
                pictureBoxDisplay.Show();
                btnExit.Show();
                _show_settings = true;
            }
        }
        //liyanchang btnShowHide Overload
        private void btnShowHide_Hover()
        {
            if (_show_settings)
            {
                tabSettings.Hide();
                pictureBoxDisplay.Hide();
                btnExit.Hide();
                _show_settings = false;
            }
            else
            {
                tabSettings.Show();
                pictureBoxDisplay.Show();
                btnExit.Show();
                _show_settings = true;
            }
        }
        private void ResetEnvironment()
        {
            labelM.Left = 35;
            labelM.Top = 9;

            labelN.Left = 35;
            labelN.Top = 35;

            labelO.Left = 9;
            labelO.Top = 9;

            labelP.Left = 9;
            labelP.Top = 35;

            gestureDemo = true;

            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            _mousedown = false;

            tabSettings.Hide();
            pictureBoxDisplay.Hide();
            btnExit.Hide();
            _show_settings = false;
            jeffHanPictureBox.Hide();//liyanchang
            flyPictureBox.Hide(); //liyanchang
            
            keyboardShortcuts(); //liyan chang

        }
        private void StopOtherApps(Object sender, EventArgs e)
        {
            if (drawDemo)
            {
                buttonDrawDemo_Click(this, e);
            }

            if (jeffDemo) //liyanchang
            {
                buttonJeffDemo_Click(this, e);
            }
            if (flyDemo)  //liyanchang
            {
                buttonFlyDemo_Click(this, e);
            }

            if (mapDemo)
            {
                buttonMapDemo_Click(this, e);
            }

            if (photoDemo)
            {
                buttonPhotoDemo_Click(this, e);
            }

            if (clockDemo)
            {
                buttonClockDemo_Click(this, e);
            }

            if (weatherDemo)
            {
                buttonWeatherDemo_Click(this, e);
            }

            if (newspaperDemo)
            {
                buttonNewsPaperDemo_Click(this, e);
            }

            if (mailDemo)
            {
                buttonMailDemo_Click(this, e);
            }

            if (menuDemo)
            {
                buttonMenuDemo_Click(this, e);
            }
            keyboardShortcuts(); //liyan chang

        }

        #endregion Environment

        #region Keyboard Events

        public void keyboardShortcuts()
        {
            this.KeyPress += new KeyPressEventHandler(onKeyPress);
        }
        public void onKeyPress(object sender, KeyPressEventArgs e)
        {
            //if (e.KeyChar == 100)
            //    buttonDrawDemo_Click(this, e);

            //if (e.KeyChar == 109)
            //    buttonMapDemo_Click(this, e);

            //if (e.KeyChar == 103)
            //    buttonGestureDemo_Click(this, e);

            //if (e.KeyChar == 112)
            //    buttonPhotoDemo_Click(this, e);

            //if (e.KeyChar == 119)
            //    buttonWeatherDemo_Click(this, e);

            //if (e.KeyChar == 99)
            //    buttonClockDemo_Click(this, e);

            //if (e.KeyChar == 111)
            //    buttonNewsPaperDemo_Click(this, e);

            //if (e.KeyChar == 104)
            //    buttonMenuDemo_Click(this, e);


            MessageBox.Show(e.KeyChar.ToString(), "INPUT 2");
        }

        #endregion Keyboard Events

        #region WUW Management
               
        public WUW()
        {
            InitializeComponent();
            
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);            
            _rec = new GeometricRecognizer();
            _points = new ArrayList(256);
            _viewFrm = null;            
        }

        private void WUW_Load(object sender, EventArgs e)
        {
 
            // Make a new TouchlessMgr for library interaction                    
            _touchlessMgr = new TouchlessMgr();

            keyboardShortcuts(); //liyan chang

            // Initialize some members
            _dtFrameLast = DateTime.Now;
            _latestFrameTime = DateTime.Now;
            _latestFrameTimeSegment = false; //liyanchang
            _fAddingMarker = false;
            _markerSelected = null;
            _addedMarkerCount = 0;
            lblMarkerCount.Text = _touchlessMgr.MarkerCount.ToString();

            //Hide Settings
            if (!_show_settings)
            {
                _show_settings = true;
                btnShowHide_Hover(this, e);
            }

            //Initialize DrawDemo Controls
            drawPanel.Hide();
            elementHostDraw.Hide();

            //Initialize MapDemo Controls
            vEarthControl1.Hide();            
            mapPanel.Hide();            
            vEarthControl1.ShowInitialMap();            

            //Initialize PhotoDemo Controls
            photoDemo_TakePhoto.Hide();
            pictureBoxAlbum.Hide();
            elementhostAlbum.Hide();

            //Initialize ClockDemo Controls        
            elementHostClock.Hide();

            //Initialize WeatherDemo Controls          
            elementHostWeather.Hide();

            //Initialize GestureDemo Controls
            gestureLoad();

            //Initialize MenuDemo Controls      
            elementHostMenu.Hide();

            //Initialize PhotoDemo WaitLabel
            handSign_WaitLabel.Hide();

            // Put the app in camera mode and select the first camera by default
            tabSettings.SelectedTab = tabPageCamera;
            foreach (Camera cam in _touchlessMgr.Cameras)
            {
                comboBoxCameras.Items.Add(cam);
            }

            if (comboBoxCameras.Items.Count > 0)
            {
                comboBoxCameras.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("WUW was unable to find a webcam. Please make sure that a camera is connected and installed.", "WUW Camera Error");
                Environment.Exit(0);
            }

            // Try going directly to the markers tab
            tabSettings.SelectedTab = tabPageTokens;
        }
        private void WUW_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Dispose of the TouchlessMgr object
            if (_touchlessMgr != null)
            {
                _touchlessMgr.Dispose();
                _touchlessMgr = null;
            }
        }
        private void WUW_Paint(object sender, PaintEventArgs e)
        {
            if (_points.Count > 0)
            {
                PointF p0 = (PointF)(PointR)_points[0]; // draw the first point bigger
                e.Graphics.FillEllipse(_recording ? Brushes.Firebrick : Brushes.DarkBlue, p0.X - 5f, p0.Y - 5f, 10f, 10f);
            }
            foreach (PointR r in _points)
            {
                PointF p = (PointF)r; // cast
                e.Graphics.FillEllipse(_recording ? Brushes.Firebrick : Brushes.DarkBlue, p.X - 2f, p.Y - 2f, 4f, 4f);
            }
        }
        private void tabSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Set or unset the picturebox mouse interaction handlers
            if (tabSettings.SelectedTab == tabPageTokens)
            {
                pictureBoxDisplay.MouseDown += new MouseEventHandler(pictureBoxDisplay_MouseDown);
                pictureBoxDisplay.MouseMove += new MouseEventHandler(pictureBoxDisplay_MouseMove);
                pictureBoxDisplay.MouseUp += new MouseEventHandler(pictureBoxDisplay_MouseUp);
            }
            else
            {
                pictureBoxDisplay.MouseDown -= new MouseEventHandler(pictureBoxDisplay_MouseDown);
                pictureBoxDisplay.MouseMove -= new MouseEventHandler(pictureBoxDisplay_MouseMove);
                pictureBoxDisplay.MouseUp -= new MouseEventHandler(pictureBoxDisplay_MouseUp);
            }            
        }

        #endregion WUW Management

        #region Touchless Event Handling

        private void drawLatestImage(object sender, PaintEventArgs e)
        {
            if (_latestFrame != null)
            {
                // Draw the latest image from the active camera
                e.Graphics.DrawImage(_latestFrame, 0, 0, pictureBoxDisplay.Width, pictureBoxDisplay.Height);

                // Draw the selection adornment
                if (_drawSelectionAdornment)
                {
                    Pen penRed = new Pen(Brushes.Red, 1);
                    e.Graphics.DrawEllipse(penRed, _markerCenter.X - _markerRadius, _markerCenter.Y - _markerRadius, 2 * _markerRadius, 2 * _markerRadius);
                }

                if (_latestFrameTimeSegment)
                {
                    e.Graphics.FillRectangle(Brushes.LightGray, 0, 0, 640, 480);  
                }
            }
        }
        public void OnImageCaptured(object sender, CameraEventArgs args)
        {
            // Calculate FPS (only update the display once every second)
            _nFrameCount++;
            double milliseconds = (DateTime.Now.Ticks - _dtFrameLast.Ticks) / TimeSpan.TicksPerMillisecond;
            if (milliseconds >= 1000)
            {
                this.BeginInvoke(new Action<double>(UpdateFPSInUI), new object[] { _nFrameCount * 1000.0 / milliseconds });
                _nFrameCount = 0;
                _dtFrameLast = DateTime.Now;
            }

            // Save the latest image for drawing
            // This happens when you are not adding markers.
            // Thus, by not doing this when you are adding markers, you freeze the frame.
            // liyanchang
            double markerWait = (DateTime.Now.Ticks - _latestFrameTime.Ticks) / TimeSpan.TicksPerMillisecond;

            if (!_fAddingMarker)
            {
                // Cause display update
                _latestFrame = args.Image;
                pictureBoxDisplay.Invalidate();
                //set time
                _latestFrameTime = DateTime.Now;
            }
            else if (_fAddingMarker && markerWait <= 5000)
            {
                // Cause display update
                _latestFrame = args.Image;   
                pictureBoxDisplay.Invalidate();
                //start timer by not setting time.

                if (markerWait % 1000 < 250)
                {
                    _latestFrameTimeSegment = true;
                }
                else
                {
                    _latestFrameTimeSegment = false;
                }
            }
        }


        private void UpdateFPSInUI(double fps)
        {
            labelCameraFPSValue.Text = "" + Math.Round(fps, 2);
            
            //TAGGING
            if (threedDemo)
            {
                nyar();
            }
            //END TAGGING.
        }
        public void OnSelectedMarkerUpdate(object sender, MarkerEventArgs args)
        {
            this.BeginInvoke(new Action<MarkerEventData>(UpdateMarkerDataInUI), new object[] { args.EventData });
        }
        private void UpdateMarkerDataInUI(MarkerEventData data)
        {
            if (data.Present)
            {
                labelMarkerData.Text =
                      "Center X:  " + data.X + "\n"
                    + "Center Y:  " + data.Y + "\n"
                    + "DX:        " + data.DX + "\n"
                    + "DY:        " + data.DY + "\n"
                    + "Area:      " + data.Area + "\n"
                    + "Left:      " + data.Bounds.Left + "\n"
                    + "Right:     " + data.Bounds.Right + "\n"
                    + "Top:       " + data.Bounds.Top + "\n"
                    + "Bottom:    " + data.Bounds.Bottom + "\n";
            }
            else
            {
                labelMarkerData.Text = "Marker not present";
            }
        }

        #endregion Touchless Event Handling

        #region Camera Mode

        private void comboBoxCameras_DropDown(object sender, EventArgs e)
        {
            // Refresh the list of available cameras
            comboBoxCameras.Items.Clear();
            foreach (Camera cam in _touchlessMgr.Cameras)
                comboBoxCameras.Items.Add(cam);
        }
        private void comboBoxCameras_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Early return if we've selected the current camera
            if (_touchlessMgr.CurrentCamera == (Camera)comboBoxCameras.SelectedItem)
                return;

            // Trash the old camera
            if (_touchlessMgr.CurrentCamera != null)
            {
                _touchlessMgr.CurrentCamera.OnImageCaptured -= new EventHandler<CameraEventArgs>(OnImageCaptured);
                _touchlessMgr.CurrentCamera.Dispose();
                _touchlessMgr.CurrentCamera = null;
                comboBoxCameras.Text = "Select A Camera";
                groupBoxCameraInfo.Enabled = false;
                groupBoxCameraInfo.Text = "No Camera Selected";
                labelCameraFPSValue.Text = "0.00";
                pictureBoxDisplay.Paint -= new PaintEventHandler(drawLatestImage);
            }

            if (comboBoxCameras.SelectedIndex < 0)
            {
                pictureBoxDisplay.Paint -= new PaintEventHandler(drawLatestImage);
                comboBoxCameras.Text = "Select A Camera";
                return;
            }

            try
            {
                Camera c = (Camera)comboBoxCameras.SelectedItem;
                c.OnImageCaptured += new EventHandler<CameraEventArgs>(OnImageCaptured);
                c.CaptureWidth = 320; //640; //960; //320;
                c.CaptureHeight = 240; //480; //720; //240;
                _touchlessMgr.CurrentCamera = c;
                _dtFrameLast = DateTime.Now;

                groupBoxCameraInfo.Enabled = true;
                groupBoxCameraInfo.Text = c.ToString();

                // Allow access to the marker mode once a camera has been activated
                // TODO: allow immediate access to the demo if we already have some markers set?

                pictureBoxDisplay.Paint += new PaintEventHandler(drawLatestImage);
            }
            catch (Exception ex)
            {
                comboBoxCameras.Text = "Select A Camera";
                MessageBox.Show(ex.Message);
            }
        }
        private void buttonCameraProperties_Click(object sender, EventArgs e)
        {
            if (comboBoxCameras.SelectedIndex < 0)
                return;

            Camera c = (Camera)comboBoxCameras.SelectedItem;
            c.ShowPropertiesDialog(this.Handle);
        }

        private void checkBoxCameraFPSLimit_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownCameraFPSLimit.Visible = numericUpDownCameraFPSLimit.Enabled = checkBoxCameraFPSLimit.Checked;
            Camera c = (Camera)comboBoxCameras.SelectedItem;
            c.Fps = checkBoxCameraFPSLimit.Checked ? (int)numericUpDownCameraFPSLimit.Value : -1;
        }
        private void numericUpDownCameraFPSLimit_ValueChanged(object sender, EventArgs e)
        {
            if (comboBoxCameras.SelectedIndex < 0)
                return;

            Camera c = (Camera)comboBoxCameras.SelectedItem;
            c.Fps = (int)numericUpDownCameraFPSLimit.Value;
        }

        #endregion Camera Mode

        #region Marker Mode

        #region Marker Buttons

        private void buttonMarkerAdd_Click(object sender, EventArgs e)
        {
            _fAddingMarker = !_fAddingMarker;
            buttonMarkerAdd.Text = _fAddingMarker ? "Cancel Adding Marker" : "Add A New Marker";
        }
        private void comboBoxMarkers_DropDown(object sender, EventArgs e)
        {
            // Refresh the marker dropdown list.
            comboBoxMarkers.Items.Clear();
            foreach (Marker marker in _touchlessMgr.Markers)
                comboBoxMarkers.Items.Add(marker);
        }
        private void comboBoxMarkers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_markerSelected != null)
                _markerSelected.OnChange -= new EventHandler<MarkerEventArgs>(OnSelectedMarkerUpdate);

            if (comboBoxMarkers.SelectedIndex < 0)
            {
                comboBoxMarkers.Text = "Edit An Existing Marker";
                groupBoxMarkerControl.Enabled = false;
                groupBoxMarkerControl.Text = "No Marker Selected";
                return;
            }

            _markerSelected = (Marker)comboBoxMarkers.SelectedItem;
            _markerSelected.OnChange += new EventHandler<MarkerEventArgs>(OnSelectedMarkerUpdate);

            groupBoxMarkerControl.Text = _markerSelected.Name;
            groupBoxMarkerControl.Enabled = true;
            _fUpdatingMarkerUI = true;
            checkBoxMarkerHighlight.Checked = _markerSelected.Highlight;
            checkBoxMarkerSmoothing.Checked = _markerSelected.SmoothingEnabled;
            numericUpDownMarkerThresh.Value = _markerSelected.Threshold;
            _fUpdatingMarkerUI = false;
        }

        #endregion Marker Buttons

        #region UI Marker Editing

        private void checkBoxMarkerHighlight_CheckedChanged(object sender, EventArgs e)
        {
            if (_fUpdatingMarkerUI)
                return;

            ((Marker)comboBoxMarkers.SelectedItem).Highlight = checkBoxMarkerHighlight.Checked;
        }
        private void checkBoxMarkerSmoothing_CheckedChanged(object sender, EventArgs e)
        {
            if (_fUpdatingMarkerUI)
                return;

            ((Marker)comboBoxMarkers.SelectedItem).SmoothingEnabled = checkBoxMarkerSmoothing.Checked;
        }
        private void numericUpDownMarkerThresh_ValueChanged(object sender, EventArgs e)
        {
            ((Marker)comboBoxMarkers.SelectedItem).Threshold = (int)numericUpDownMarkerThresh.Value;
        }
        private void buttonMarkerRemove_Click(object sender, EventArgs e)
        {
            while (_touchlessMgr.MarkerCount != 0)
            {
                _touchlessMgr.RemoveMarker(0);
                comboBoxMarkers.Items.RemoveAt(0);
            }

            comboBoxMarkers.SelectedIndex = -1;
            comboBoxMarkers.Text = "Edit An Existing Marker";
            groupBoxMarkerControl.Enabled = false;
            groupBoxMarkerControl.Text = "No Marker Selected";

            comboBoxMarkers.Enabled = false;

            lblMarkerCount.Text = _touchlessMgr.MarkerCount.ToString();

            //_touchlessMgr.RemoveMarker(comboBoxMarkers.SelectedIndex);
            //comboBoxMarkers.Items.RemoveAt(comboBoxMarkers.SelectedIndex);
            //comboBoxMarkers.SelectedIndex = -1;
            //comboBoxMarkers.Text = "Edit An Existing Marker";
            //groupBoxMarkerControl.Enabled = false;
            //groupBoxMarkerControl.Text = "No Marker Selected";
            //if (comboBoxMarkers.Items.Count == 0)
            //{
            //    //radioButtonDemo.Enabled = false;
            //    comboBoxMarkers.Enabled = false;
            //}
            //lblMarkerCount.Text = _touchlessMgr.MarkerCount.ToString();
        }

        //liyanchang
        private void buttonMarkerSave_Click(object sender, EventArgs e)
        {
            if (_touchlessMgr.MarkerCount >= 4)
            {
                TextWriter tw = new StreamWriter("markerSave.txt");
                tw.WriteLine(m.Threshold);
                tw.WriteLine(n.Threshold);
                tw.WriteLine(o.Threshold);
                tw.WriteLine(p.Threshold);
                tw.Close();
            }
            else
            {
                MessageBox.Show("No Marker Data Saved. Please note that 4 markers are required to save.");
            }       
        }

        //liyanchang
        private void buttonMarkerLoad_Click(object sender, EventArgs e)
        {
            StreamReader read = File.OpenText("markerSave.txt");
            string[] markerSaveArray = new string[4];
            for (int x = 0; x <4 ; x++)
            {
                markerSaveArray[x] = read.ReadLine();
            }
            read.Dispose();

            _touchlessMgr.Markers.ElementAt(0).Threshold = Convert.ToInt32(markerSaveArray[0]);
            _touchlessMgr.Markers.ElementAt(1).Threshold = Convert.ToInt32(markerSaveArray[1]);
            _touchlessMgr.Markers.ElementAt(2).Threshold = Convert.ToInt32(markerSaveArray[2]);
            _touchlessMgr.Markers.ElementAt(3).Threshold = Convert.ToInt32(markerSaveArray[3]);

        }

        #endregion UI Marker Editing

        #region Display Interaction

        private void pictureBoxDisplay_MouseDown(object sender, MouseEventArgs e)
        {
            // If we are adding a marker - get the marker center on mouse down
            if (_fAddingMarker)
            {
                _markerCenter = e.Location;
                _markerRadius = 0;

                // Begin drawing the selection adornment
                _drawSelectionAdornment = true;
            }
        }
        private void pictureBoxDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            // If the user is selecting a marker, draw a circle of their selection as a selection adornment
            if (_fAddingMarker && !_markerCenter.IsEmpty)
            {
                // Get the current radius
                int dx = e.X - _markerCenter.X;
                int dy = e.Y - _markerCenter.Y;
                _markerRadius = (float)Math.Sqrt(dx * dx + dy * dy);

                // Cause display update
                pictureBoxDisplay.Invalidate();
            }
        }
        private void pictureBoxDisplay_MouseUp(object sender, MouseEventArgs e)
        {
            // If we are adding a marker - get the marker radius on mouse up, add the marker

            //liyanchang
            //for some reason, wuw sends through 2 mouseup events. however, we can null the second by adding another stipulation.
            //!_markerCenter.IsEmpty.
            if (_fAddingMarker && !_markerCenter.IsEmpty)
            {
                int dx = e.X - _markerCenter.X;
                int dy = e.Y - _markerCenter.Y;
                _markerRadius = (float)Math.Sqrt(dx * dx + dy * dy);

                // Adjust for the image/display scaling (assumes proportional scaling)
                _markerCenter.X = (_markerCenter.X * _latestFrame.Width) / pictureBoxDisplay.Width;
                _markerCenter.Y = (_markerCenter.Y * _latestFrame.Height) / pictureBoxDisplay.Height;
                _markerRadius = (_markerRadius * _latestFrame.Height) / pictureBoxDisplay.Height;

                // Add the marker
                Marker newMarker = _touchlessMgr.AddMarker("Marker #" + ++_addedMarkerCount, (Bitmap)_latestFrame, _markerCenter, _markerRadius);
                lblMarkerCount.Text = _touchlessMgr.MarkerCount.ToString();
                comboBoxMarkers.Items.Add(newMarker);

                // Restore the app to its normal state and clear the selection area adorment
                //Liyan Chang.
                //Only if there are four.

                _markerCenter = new Point(); //remove the old point
                _drawSelectionAdornment = false; 
                pictureBoxDisplay.Invalidate(); //to clear up ellipses.

                if (_touchlessMgr.MarkerCount == 4)
                {
                    _fAddingMarker = false;
                    buttonMarkerAdd.Text = "Add A New Marker";
                    pictureBoxDisplay.Image = new Bitmap(pictureBoxDisplay.Width, pictureBoxDisplay.Height);

                    // Enable the demo and marker editing             
                    comboBoxMarkers.Enabled = true;
                }

                //checks if there are 4 markers and name them
                nameMarkers();
            }
        }

        #endregion Display Interaction

        #endregion Marker Mode

        #region Marker Functions

        #region Marker Initial Functions

        private void nameMarkers()
        {
            //This function is to name the added markers to m,n,o,p.
            //This is run after the fourth marker is added. (pictureboxdisplay_mouseup)
            if (_touchlessMgr.MarkerCount == 4)
            {
                m = _touchlessMgr.Markers.ElementAt(0);
                n = _touchlessMgr.Markers.ElementAt(1);
                o = _touchlessMgr.Markers.ElementAt(2);
                p = _touchlessMgr.Markers.ElementAt(3);

                m.OnChange += new EventHandler<MarkerEventArgs>(m_OnChange);
                n.OnChange += new EventHandler<MarkerEventArgs>(n_OnChange);
                o.OnChange += new EventHandler<MarkerEventArgs>(o_OnChange);
                p.OnChange += new EventHandler<MarkerEventArgs>(p_OnChange);

                _ratioScreenCameraHeight = 768 / _touchlessMgr.CurrentCamera.CaptureHeight;
                _ratioScreenCameraWidth = 1024 / _touchlessMgr.CurrentCamera.CaptureWidth;
            }
        }

        public void testKeyPress(object sender, KeyEventArgs e)
        {

            MessageBox.Show(e.KeyCode.ToString(), "Your input");

        }

        #endregion Marker Initial Functions

        #region Marker_OnChange

        void m_OnChange(object sender, MarkerEventArgs e)
        {

            if (jeffDemo)
            {
                PainterForm_MouseMove(0, e.EventData.X * _ratioScreenCameraWidth, e.EventData.Y * _ratioScreenCameraHeight);
            }
            else
            {
                updatelabelMLocation(new Point(e.EventData.X * _ratioScreenCameraWidth, e.EventData.Y * _ratioScreenCameraHeight));
                Cursor.Position = new Point(labelM.Location.X - 1, labelM.Location.Y - 1);
            }

            //If the N marker is not present, the mouse will left click.
            handSign_NoN();

            if (!menuDemo && handSign_Menu())
                    RunMenuGesture(new EventArgs());

            if (photoDemo && handSign_TakePicture())                
                    RunPhotoGesture(new EventArgs());

            if (mapDemo)
                mapDemo_OnChange();



            if (flyDemo)
            {
                flyDemo_MouseMove(labelM.Location, labelN.Location);
            }

            //TLIMAGE
            if (newspaperDemo)
            {
                updateImageM(this, e);
                if (handSign_ImageTrigger())
                {
                    buttonJeffDemo_Click(this, new EventArgs());
                }
            }
        }
        void n_OnChange(object sender, MarkerEventArgs e)
        {
            if (jeffDemo)
            {
                PainterForm_MouseMove(1, e.EventData.X * _ratioScreenCameraWidth, e.EventData.Y * _ratioScreenCameraHeight);
            }
            else
            {
                updatelabelNLocation(new Point(e.EventData.X * _ratioScreenCameraWidth, e.EventData.Y * _ratioScreenCameraHeight));
            }
        }
        void o_OnChange(object sender, MarkerEventArgs e)
        {
            if (jeffDemo)
            {
                PainterForm_MouseMove(2, e.EventData.X * _ratioScreenCameraWidth, e.EventData.Y * _ratioScreenCameraHeight);
            }
            else
            {
                updatelabelOLocation(new Point(e.EventData.X * _ratioScreenCameraWidth, e.EventData.Y * _ratioScreenCameraHeight));
            }

            //TLIMAGE
            if (newspaperDemo)
            {
                updateImageO(this, e);
            }
        
        }
        void p_OnChange(object sender, MarkerEventArgs e)
        {
            if (jeffDemo)
            {
                PainterForm_MouseMove(3, e.EventData.X * _ratioScreenCameraWidth, e.EventData.Y * _ratioScreenCameraHeight);
            }
            else
            {
                updatelabelPLocation(new Point(e.EventData.X * _ratioScreenCameraWidth, e.EventData.Y * _ratioScreenCameraHeight));
            }
        }

        #endregion Marker_OnChange

        #region UpdateLabelLocation

        delegate void updateLabelLocationDelegate(Point newPoint);
        delegate void runMenuGestureDelegate(EventArgs newe);
        delegate void runPhotoGestureDelegate(EventArgs newe);
        delegate void runTimerDelegate(EventArgs moree);

        private void RunMenuGesture(EventArgs newe)
        {
            if (buttonMenuDemo.InvokeRequired)
            {
                runMenuGestureDelegate delMenu = new runMenuGestureDelegate(RunMenuGesture);
                buttonMenuDemo.Invoke(delMenu, new object[] { newe });
            }
            else
            {
                buttonMenuDemo_Click(this, newe);
            }
        }
        private void RunPhotoGesture(EventArgs photoe)
        {
            if (photoDemo_TakePhoto.InvokeRequired)
            {
                runPhotoGestureDelegate delPhoto = new runPhotoGestureDelegate(RunPhotoGesture);
                photoDemo_TakePhoto.Invoke(delPhoto, new object[] { photoe });
            }
            else
            {
                photoDemo_TakePhoto_Hover(this, photoe);
            }
        }

        private void updatelabelMLocation(Point newPoint)
        {
            if (labelM.InvokeRequired)
            {
                updateLabelLocationDelegate del = new updateLabelLocationDelegate(updatelabelMLocation);
                labelM.Invoke(del, new object[] { newPoint });
            }
            else
            {
                labelM.Location = newPoint;
            }

        }
        private void updatelabelNLocation(Point newPoint)
        {
            if (labelN.InvokeRequired)
            {
                updateLabelLocationDelegate del = new updateLabelLocationDelegate(updatelabelNLocation);
                labelN.Invoke(del, new object[] { newPoint });
            }
            else
            {
                labelN.Location = newPoint;
            }
        }
        private void updatelabelOLocation(Point newPoint)
        {
            if (labelO.InvokeRequired)
            {
                updateLabelLocationDelegate del = new updateLabelLocationDelegate(updatelabelOLocation);
                labelO.Invoke(del, new object[] { newPoint });
            }
            else
            {
                labelO.Location = newPoint;
            }
        }
        private void updatelabelPLocation(Point newPoint)
        {
            if (labelP.InvokeRequired)
            {
                updateLabelLocationDelegate del = new updateLabelLocationDelegate(updatelabelPLocation);
                labelP.Invoke(del, new object[] { newPoint });
            }
            else
            {
                labelP.Location = newPoint;
            }
        }

        #endregion UpdateLabelLocation

        #region Marker Helper Functions

        private long calculateDistance(Point a, Point b)
        {
            long distance =
                Math.Abs(
                    (a.X - b.X) ^ 2 +
                    (a.Y - b.Y) ^ 2
                    );
            return distance;
        }
        private long calculateDistance(int aX, int aY, int bX, int bY)
        {
            long distance =
                Math.Abs(
                    (aX - bX) ^ 2 +
                    (aY - bY) ^ 2
                    );
            return distance;
        }

        private Point averagePoint(Point a, Point b)
        {
            return new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2);
        }

        private void handSign_Wait(string text, int time)
        {
            handSign_WaitLabel.Show();
            handSign_WaitLabel.Text = text;
            handSign_WaitLabel.Update();
            
            while (time> 0)
            {
                   //liyanchang
                System.Media.SystemSounds.Beep.Play();
                System.Threading.Thread.Sleep(500);
                handSign_WaitLabel.Text = time.ToString();
                handSign_WaitLabel.Update();
                time--;
                System.Threading.Thread.Sleep(500);
                handSign_WaitLabel.Text = text;
                handSign_WaitLabel.Update();
            }
            handSign_WaitLabel.Hide();
        }

        #endregion Marker Helper Functions

        #region Marker HandSigns Functions

        // TLIMAGE
        private bool handSign_ImageTrigger()
        {
            if (calculateDistance(labelM.Location, labelO.Location) < 50)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool handSign_NoN()
        {
            if (!n.PreviousData.Present && !_mousedown)
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, labelM.Location.X - 10, labelM.Location.Y - 10, 0, 0);
                _mousedown = true;
                return true;
            }
            else if (n.PreviousData.Present && _mousedown)
            {
                mouse_event(MOUSEEVENTF_LEFTUP, labelM.Location.X - 10, labelM.Location.Y - 10, 0, 0);
                _mousedown = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool handSign_Menu()
        {

            if (((DateTime.Now.Ticks - menuStart) / TimeSpan.TicksPerMillisecond) > 2000)
            {
                menuStart = null;
                return true;
            }

            float distance1 = calculateDistance(labelM.Location, labelO.Location);
            float distance2 = calculateDistance(labelN.Location, labelP.Location);
            float distanceBetweenPairs = calculateDistance(labelM.Location, labelN.Location);

            if (distance1 < 150 && distance2 < 150 && distanceBetweenPairs > 150)
            {
                if (menuStart == null)
                    menuStart = DateTime.Now.Ticks;
                return false;
            }
            else
            {
                menuStart = null;
                return false;
            }
        }
        //void handSign_scaleMap()
        //{
        //    if (m.PreviousData.Present && n.PreviousData.Present && o.PreviousData.Present)
        //    {
        //        long distance = calculateDistance(labelM.Location, labelO.Location);

        //        if (!_zoomtoggle)
        //        {
        //            if (distance < 100)
        //            {
        //                vEarthControl1.ZoomOut();
        //                _zoomtoggle = true;
        //            }
        //            else if (distance > 900)
        //            {
        //                vEarthControl1.ZoomIn();
        //                _zoomtoggle = true;
        //            }
        //        }
        //        if (distance < 700 && distance > 300 && _zoomtoggle)
        //            _zoomtoggle = false;
        //    }
        //}

        long distanceBetweenInitial = 0;
        long zoomFactor = 1;

        void handSign_scaleMap()
        {
            if (m.PreviousData.Present && n.PreviousData.Present && o.PreviousData.Present && p.PreviousData.Present)
            {
                long distance1 = calculateDistance(labelM.Location, labelN.Location);
                long distance2 = calculateDistance(labelO.Location, labelP.Location);

                Point zoomPoint = new Point();
                Point pointBetweenFinal = new Point();
                Point pointBetweenInitial = new Point();

                if (!_zoomtoggle && distance1 < 80 && distance2 < 80)
                {
                    _zoomtoggle = true;
                    distanceBetweenInitial = calculateDistance(labelM.Location, labelO.Location);
                    pointBetweenInitial = averagePoint(labelM.Location, labelO.Location);
                   // MessageBox.Show("trigger");
                }

                if (_zoomtoggle && distance1 > 120 && distance2 > 120)
                {
                    _zoomtoggle = false;
                    zoomFactor = calculateDistance(labelM.Location, labelO.Location) / distanceBetweenInitial;
                    pointBetweenFinal = averagePoint(labelM.Location, labelO.Location);
                    zoomPoint = averagePoint(pointBetweenInitial, pointBetweenFinal);

                    //MessageBox.Show("zoom" + zoomFactor + "/n" + zoomPoint.X + "," + zoomPoint.Y );

                    //Current bad bindings to the map api.
                    if (zoomFactor > 1)
                        vEarthControl1.ZoomIn();

                    if (zoomFactor < 1)
                        vEarthControl1.ZoomOut();
                    zoomFactor = 1;
                }
                

            }
        }

        bool handSign_TakePicture()
        {
            double timeElapsed = (DateTime.Now.Ticks - photoTaken) / TimeSpan.TicksPerMillisecond;

            long distance = calculateDistance(labelN.Location, labelO.Location) +
                calculateDistance(labelM.Location, labelP.Location);
            if (distance < 150)
            {
                if (timeElapsed > 6000)  //3 second relod time.
                {
                    photoTaken = DateTime.Now.Ticks;
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        #endregion Marker HandSigns Functions

        #endregion Marker Functions

        #region Gesture Buttons

        private void btnRecord_Click(object sender, EventArgs e)
        {
            _points.Clear();
            Invalidate();
            _recording = !_recording; // recording will happen on mouse-up
            lblRecord.Visible = _recording;
        }
        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Gestures (*.xml)|*.xml";
            dlg.Title = "Load Gestures";
            dlg.Multiselect = true;
            dlg.RestoreDirectory = false;
            dlg.InitialDirectory = "Gestures";


            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                for (int i = 0; i < dlg.FileNames.Length; i++)
                {
                    string name = dlg.FileNames[i];
                    _rec.LoadGesture(name);
                }
                ReloadViewForm();
                Cursor.Current = Cursors.Default;
            }

        }
        private void btnView_Click(object sender, EventArgs e)
        {
            if (_viewFrm != null && !_viewFrm.IsDisposed)
            {
                _viewFrm.Close();
                _viewFrm = null;
            }
            else
            {
                Cursor.Current = Cursors.WaitCursor;
                _viewFrm = new ViewForm(_rec.Gestures);
                _viewFrm.Owner = this;
                _viewFrm.Show();
                Cursor.Current = Cursors.Default;
            }
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "This will clear all loaded gestures. (It will not delete any XML files.)", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                _rec.ClearGestures();
                ReloadViewForm();
            }
        }

        #endregion Gesture Buttons

        #region Gesture Functions

        private void ReloadViewForm()
        {
            if (_viewFrm != null && !_viewFrm.IsDisposed)
            {
                _viewFrm.Close();
                _viewFrm = new ViewForm(_rec.Gestures);
                _viewFrm.Owner = this;
                _viewFrm.Show();
            }
        }
        private void gestureLoad()
        {
            string folderName = "Gestures";
            string[] filePath = Directory.GetFiles(folderName, "*.xml");

            foreach(string fileName in filePath)
                _rec.LoadGesture(fileName);

            ReloadViewForm();
            Cursor.Current = Cursors.Default;

        }

        #endregion Gesture Functions

        #region Gesture Mouse Events

        private void WUW_MouseDown(object sender, MouseEventArgs e)
        {
            _isDown = true;
            _points.Clear();
            _points.Add(new PointR(e.X, e.Y, Environment.TickCount));
            Invalidate();
        }
        private void WUW_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDown)
			{
				_points.Add(new PointR(e.X, e.Y, Environment.TickCount));
				Invalidate(new Rectangle(e.X - 2, e.Y - 2, 4, 4));
			}
		}
        private void WUW_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isDown)
            {
                _isDown = false;

                if (_points.Count >= 5) // require 5 points for a valid gesture
                {
                    if (_recording)
                    {
                        SaveFileDialog dlg = new SaveFileDialog();
                        dlg.Filter = "Gestures (*.xml)|*.xml";
                        dlg.Title = "Save Gesture As";
                        dlg.AddExtension = true;
                        dlg.RestoreDirectory = false;
                        dlg.InitialDirectory = "Gestures";

                        if (dlg.ShowDialog(this) == DialogResult.OK)
                        {
                            _rec.SaveGesture(dlg.FileName, _points);  // resample, scale, translate to origin
                            ReloadViewForm();
                        }

                        dlg.Dispose();
                        _recording = false;
                        lblRecord.Visible = false;
                        Invalidate();
                    }
                    else if (_rec.NumGestures > 0) // not recording, so testing
                    {                        
                        Application.DoEvents(); // forces label to display

                        NBestList result = _rec.Recognize(_points); // where all the action is!!
                        lblResult.Text = String.Format("{0}: {1} ({2}px, {3}{4})",
                            result.Name,
                            Math.Round(result.Score, 2),
                            Math.Round(result.Distance, 2),
                            Math.Round(result.Angle, 2), (char)176);

                        switch (result.Name)
                        {
                            case "clock1":
                            case "clock2":
                                buttonClockDemo_Click(this, e);
                                break;
                            case "draw1":
                            case "draw2":
                            case "draw3":
                            case "draw4":
                                buttonDrawDemo_Click(this, e);
                                break;
                            case "email":
                                break;
                            case "map1":
                            case "map2":
                                buttonMapDemo_Click(this, e);
                                break;
                            case "menu1":
                            case "menu2":
                            case "menuSQ1":
                            case "menuSQ1b":
                            case "menuSQ2":
                                buttonMenuDemo_Click(this, e);
                                break;
                            case "photo1":
                            case "photo2":
                            case "photo3":
                            case "photo4":
                            case "photo5":
                            case "photo6":
                                buttonPhotoDemo_Click(this, e);
                                break;
                            case "weather1":
                            case "weather2":
                                buttonWeatherDemo_Click(this, e);
                                break;
                            default:
                                break;
                        }

                    }
                }
            }
        }

		#endregion Gesture Mouse Events

        #region Demo Mode

        #region Draw Demo

        private void buttonDrawDemo_Click(object sender, EventArgs e)
        {
                if (drawDemo == false)
                {
                    StopOtherApps(this, e);
                    inkCanvas1 = new System.Windows.Controls.InkCanvas();
                    inkCanvas1.Background = System.Windows.Media.Brushes.Transparent;
                    inkCanvas1.DefaultDrawingAttributes.Color = System.Windows.Media.Colors.White;
                    inkCanvas1.DefaultDrawingAttributes.Width = 16;
                    inkCanvas1.DefaultDrawingAttributes.Height = 16;

                    inkCanvas2 = new System.Windows.Controls.InkCanvas();
                    inkCanvas2.Background = System.Windows.Media.Brushes.Transparent;
                    inkCanvas2.DefaultDrawingAttributes.Color = System.Windows.Media.Colors.Blue;
                    inkCanvas2.DefaultDrawingAttributes.Width = 60;
                    inkCanvas2.DefaultDrawingAttributes.Height = 60;

                    elementHostDraw.Child = inkCanvas1;
                    
                    drawDemo = true;
                    labelDemoName.Text = "Draw";
                    buttonDrawDemo.Text = "Stop Draw";
                    //inkCanvasLoad();
                    elementHostDraw.Show();
                    drawPanel.Show();
                    labelDemoInstructions.Enabled = true;
                    labelDemoInstructions.Text = "Draw Demo Instructions:\n\n"
                        + "Draws with Marker M when N is not\n"
                        + "present.";
                    btnShowHide_Hover();//liyanchang


                }
                else
                {
                    drawDemo = false;
                    labelDemoName.Text = "WUW";
                    buttonDrawDemo.Text = "Draw";
                    inkCanvasSaveJpeg();
                    elementHostDraw.Hide();
                    drawPanel.Hide();
                    Cursor = Cursors.Arrow;
                    labelDemoInstructions.Enabled = false;
                    labelDemoInstructions.Text = "";
                    ResetEnvironment();
                }
        }

        private void inkCanvasColor_Hover(object sender, EventArgs e)
        {
            Color inkColor = ((Control)sender).BackColor;
            inkCanvas1.DefaultDrawingAttributes.Color = System.Windows.Media.Color.FromArgb(inkColor.A, inkColor.R, inkColor.G, inkColor.B);
        }
        private void inkCanvasThinner_Hover(object sender, EventArgs e)
        {
            if (inkCanvas1.EditingMode != System.Windows.Controls.InkCanvasEditingMode.EraseByPoint)
            {
                if (inkCanvas1.DefaultDrawingAttributes.Height >= 4)
                {
                    inkCanvas1.DefaultDrawingAttributes.Height -= 2;
                    inkCanvas1.DefaultDrawingAttributes.Width -= 2;
                }
            }
            else
            {
                if (inkCanvas1.EraserShape.Height >= 2)
                    inkCanvas1.EraserShape = new System.Windows.Ink.RectangleStylusShape(inkCanvas1.EraserShape.Height - 2, inkCanvas1.EraserShape.Width - 2);
            }
        }
        private void inkCanvasThinner_Click(object sender, EventArgs e)
        {
            if (inkCanvas1.EditingMode != System.Windows.Controls.InkCanvasEditingMode.EraseByPoint)
            {
                if (inkCanvas1.DefaultDrawingAttributes.Height >= 14)
                {
                    inkCanvas1.DefaultDrawingAttributes.Height -= 10;
                    inkCanvas1.DefaultDrawingAttributes.Width -= 10;
                }
            }
            else
            {
                if (inkCanvas1.EraserShape.Height >= 10)
                    inkCanvas1.EraserShape = new System.Windows.Ink.RectangleStylusShape(inkCanvas1.EraserShape.Height - 10, inkCanvas1.EraserShape.Width - 10);
            }
        }
        private void inkCanvasThicker_Hover(object sender, EventArgs e)
        {
            if (inkCanvas1.EditingMode != System.Windows.Controls.InkCanvasEditingMode.EraseByPoint)
            {
                inkCanvas1.DefaultDrawingAttributes.Height += 2;
                inkCanvas1.DefaultDrawingAttributes.Width += 2;
            }
            else
            {
                inkCanvas1.EraserShape = new System.Windows.Ink.RectangleStylusShape(inkCanvas1.EraserShape.Height + 2, inkCanvas1.EraserShape.Width + 2);
            }
        }
        private void inkCanvasThicker_Click(object sender, EventArgs e)
        {
            if (inkCanvas1.EditingMode != System.Windows.Controls.InkCanvasEditingMode.EraseByPoint)
            {
                inkCanvas1.DefaultDrawingAttributes.Height += 10;
                inkCanvas1.DefaultDrawingAttributes.Width += 10;
            }
            else
            {
                inkCanvas1.EraserShape = new System.Windows.Ink.RectangleStylusShape(inkCanvas1.EraserShape.Height + 10, inkCanvas1.EraserShape.Width + 10);
            }
        }
        private void inkCanvasToggle_Hover(object sender, EventArgs e)
        {
            if (inkCanvas1.EditingMode != System.Windows.Controls.InkCanvasEditingMode.EraseByPoint)
            {
                inkCanvas1.EditingMode = System.Windows.Controls.InkCanvasEditingMode.EraseByPoint;
                inkCanvasToggle.Text = "!";
                //#
            }
            else
            {
                inkCanvas1.EditingMode = System.Windows.Controls.InkCanvasEditingMode.Ink;
                inkCanvasToggle.Text = "x";
                //%
            }
        }

        private void inkCanvasSave()
        {
            // Specify the folder and file your ink data will be stored in 
            string folderName = "Inkings";
            string filePath = folderName + "\\savedInkStrokes.ink";

            // Check if directory exists 
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            // Create a new file (or overwrite an existing one) to store our data 
            FileStream inkCanvasFileStream = new FileStream(filePath, FileMode.Create);

            // Transfer your data and close the file. 
            inkCanvas1.Strokes.Save(inkCanvasFileStream);
            inkCanvasFileStream.Close();
        }
        private void inkCanvasSaveJpeg()
        {
            //This will save two 

            // Specify the folder and file your ink data will be stored in 
            string folderName = "Inkings";
            string filePath;

            if (Directory.Exists(folderName))
            {
                filePath = folderName + "\\" + Directory.GetFiles(folderName).Length.ToString();
                if (File.Exists(filePath + ".ink"))
                { 
                    filePath += "(copy)"; 
                }
            }
            else
            {
                Directory.CreateDirectory("Inkings");
                filePath = folderName + "\\1";
            }

            //save the strokes
            FileStream inkCanvasFileStreamInk = new FileStream(filePath + ".ink", FileMode.Create);
            inkCanvas1.Strokes.Save(inkCanvasFileStreamInk);
            inkCanvasFileStreamInk.Close();

            //render the strokes for JPEG
            FileStream inkCanvasFileStream = new FileStream(filePath+".jpg", FileMode.Create);
            int marg = int.Parse(this.inkCanvas1.Margin.Left.ToString());
            RenderTargetBitmap rtb =
                    new RenderTargetBitmap((int)this.inkCanvas1.ActualWidth - marg,
                            (int)this.inkCanvas1.ActualHeight - marg, 0, 0,
                        System.Windows.Media.PixelFormats.Default);
            rtb.Render(this.inkCanvas1);
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(inkCanvasFileStream);
            inkCanvasFileStream.Close();
            
        }
        private void inkCanvasLoad(string filePath)
        {

            // If our file exists, 
            if (File.Exists(filePath))
            {
                FileStream inkCanvasFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                StrokeCollection savedInkStrokes = new StrokeCollection(inkCanvasFileStream);
                inkCanvasFileStream.Close();

                inkCanvas1.Strokes = savedInkStrokes;
            }
        }

        #endregion Draw Demo

        #region Map Demo

        private void buttonMapDemo_Click(object sender, EventArgs e)
        {
                if (mapDemo == false)
                {
                    StopOtherApps(this, e);
                    mapDemo = true;
                    labelDemoName.Text = "Map";
                    buttonMapDemo.Text = "Stop Map";
                    mapPanel.Show();
                    vEarthControl1.Show();
                    vEarthControl1.DashBoardHide();                    
                    //vEarthControl1.GoToCoordinates(4.236260292172E+15, -7.10911560058594E+15);                                      
                    Cursor = Cursors.Hand;
                    labelDemoInstructions.Enabled = true;
                    labelDemoInstructions.Text = "Map Demo Instructions:\n";
                    btnShowHide_Hover(); //liyanchang
                }
                else
                {
                    mapDemo = false;
                    labelDemoName.Text = "WUW";
                    buttonMapDemo.Text = "Map";
                    mapPanel.Hide();
                    vEarthControl1.Hide();
                    Cursor = Cursors.Arrow;
                    labelDemoInstructions.Enabled = false;
                    labelDemoInstructions.Text = "";
                    ResetEnvironment();
                }
        }
        private void mapDemo_OnChange()
        {
            handSign_scaleMap();
        }

        private void vEarthControl1_OnMoveOnMap(object sender, VEarth.OnMoveOnMapEventArgs e)
        {
            //label1.Text = "Current position: " + e.Lat.ToString() + "; " + e.Lon.ToString();
        }
        private void vEarthControl1_OnClickOnMap(object sender, VEarth.OnClickOnMapEventArgs e)
        {
            //label1.Text = "Click at: " + e.Lat.ToString() + "; " + e.Lon.ToString();
        }

        private void vEarthZoomIn_Hover(object sender, EventArgs e)
        {
            //mouse_event(MOUSEEVENTF_WHEEL, 100, 100, -1, -1);
            vEarthControl1.ZoomIn();
        }
        private void vEarthZoomOut_Hover(object sender, EventArgs e)
        {
            vEarthControl1.ZoomOut();
        }
        private void vEarthToggleMode_Hover(object sender, EventArgs e)
        {
            if (_mapmoderoad)
            {
                vEarthControl1.SetMapStyle(VEarth.VEarthControl.MapStyleEnum.Aerial);
                _mapmoderoad = false;
            }
            else
            {
                vEarthControl1.SetMapStyle(VEarth.VEarthControl.MapStyleEnum.Road);
                _mapmoderoad = true;
            }
        }


        #endregion Map Demo

        #region Photo Demo

        private void buttonPhotoDemo_Click(object sender, EventArgs e)
        {
            if (photoDemo == false)
            {
                StopOtherApps(this, e);                
                //elementhostAlbum.Child = Control_album;
                photoDemo_TakePhoto.Show();
                photoDemo = true;
                labelDemoName.Text = "Photo";
                buttonPhotoDemo.Text = "Stop Photo";
                pictureBoxAlbum.Show();
                //elementhostAlbum.Show();
                Cursor = Cursors.Hand;
                labelDemoInstructions.Enabled = true;
                labelDemoInstructions.Text = "Photo Demo Instructions:\n\n"
                    + "Use two markers to move two pointers.\n"
                    + "Move them close together to click.";                    
            }
            else
            {
                photoDemo = false;
                labelDemoName.Text = "WUW";
                buttonPhotoDemo.Text = "Photo";
                pictureBoxAlbum.Hide();
                photoDemo_TakePhoto.Hide();
                //elementhostAlbum.Hide();
                Cursor = Cursors.Arrow;
                labelDemoInstructions.Enabled = false;
                labelDemoInstructions.Text = "";
                ResetEnvironment();
            }
        }
        void PhotoDemo_TakePicture()
        {
            Bitmap _latestFrame_Resize = new Bitmap(_latestFrame, 640, 480);
            pictureBoxAlbum.Image = _latestFrame_Resize;
            if (Directory.Exists("pics"))
            {
                string newimage = "pics//" + Directory.GetFiles("pics").Length.ToString() + ".jpg";
                _latestFrame.Save(newimage, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else
            {
                Directory.CreateDirectory("pics");
                _latestFrame.Save("pics//1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }
        private void photoDemo_TakePhoto_Hover(object sender, EventArgs e)
        {
            TimerDemo();
            handSign_Wait("Photo", 3);
        }

        public void TimerDemo()
        {
            Timer = new System.Windows.Forms.Timer();
            Timer.Interval = 1000;
            Timer.Start();
            Timer.Tick += new EventHandler(Timer_Tick);
        }
        public void Timer_Tick(object sender, EventArgs eArgs)
        {
            if (timerSum >= 1)
            {
                Timer.Stop();
                Timer.Dispose();
                timerSum = 0;
                System.Media.SystemSounds.Asterisk.Play(); //liyanchang
                PhotoDemo_TakePicture();
            }
            else
            {
                timerSum++;
            }
        }

        #endregion Photo Demo

        #region Clock Demo

        private void buttonClockDemo_Click(object sender, EventArgs e)
        {
            if (clockDemo == false)
            {
                StopOtherApps(this, e);
                elementHostClock.Child = Control_clock;
                
                clockDemo = true;
                labelDemoName.Text = "Clock";
                buttonClockDemo.Text = "Stop Clock";
                elementHostClock.Show();                
                labelDemoInstructions.Enabled = true;
                labelDemoInstructions.Text = "Clock Demo Instructions:\n\n"
                    + "Clock follows Marker M.";                    

            }
            else
            {
                clockDemo = false;
                labelDemoName.Text = "WUW";
                buttonClockDemo.Text = "Clock";
                elementHostClock.Hide();                
                Cursor = Cursors.Arrow;
                labelDemoInstructions.Enabled = false;
                labelDemoInstructions.Text = "";
                ResetEnvironment();
            }
        }     
        #endregion Clock Demo

        #region Weather Demo

        private void buttonWeatherDemo_Click(object sender, EventArgs e)
        {
            
            if (weatherDemo == false)
            {
                StopOtherApps(this, e);
                elementHostWeather.Child = Control_weather;

                weatherDemo = true;
                labelDemoName.Text = "Weather";
                buttonWeatherDemo.Text = "Stop Weather";
                elementHostWeather.Show();
                labelDemoInstructions.Enabled = true;
                labelDemoInstructions.Text = "Weather Demo Instructions:\n\n"
                    + "Weather Demo";                   

            }
            else
            {
                weatherDemo = false;
                labelDemoName.Text = "WUW";
                buttonWeatherDemo.Text = "Weather";
                elementHostWeather.Hide();
                Cursor = Cursors.Arrow;
                labelDemoInstructions.Enabled = false;
                labelDemoInstructions.Text = "";
                ResetEnvironment();
            }
        }

        #endregion Clock Demo

        #region NewsPaper Demo
        //TLIMAGE

        //Initialize stuff

        private Bitmap _image;
        private int _captureWidth, _captureHeight;
        private int _displayWidth, _displayHeight;
        private float _displayScale;

        // For multiple markers (upper-left, upper-right, and lower-left corners)
        private Point[] _destPoints;
        private int _imageWidth, _imageHeight;
        private float _imageDiagonal, _imageScale;
        // The angle from the lower-left corner to the upper right corner (from North)
        private float _imageDiagonalAngle;

        private void buttonNewsPaperDemo_Click(object sender, EventArgs e)
        {
            if (newspaperDemo == false)
            {
                StopOtherApps(this, e);
                newspaperDemo = true;
                labelDemoName.Text = "NewsPaper";
                buttonNewsPaperDemo.Text = "Stop NewsPaper";
                labelDemoInstructions.Enabled = true;
                labelDemoInstructions.Text = "NewsPaper Demo Instructions:\n\n"
                    + "NewsPaper Video.\n";

                if (_touchlessMgr.MarkerCount >= 2)
                {

                    //Set the images
                    _image = new Bitmap("pics/0.jpg");
                    _imageWidth = _image.Width;
                    _imageHeight = _image.Height;

                    // Initialize the bounds
                    _captureWidth = _touchlessMgr.CurrentCamera.CaptureWidth;
                    _captureHeight = _touchlessMgr.CurrentCamera.CaptureHeight;
                    _displayWidth = 1024;
                    _displayHeight = 768;
                    _displayScale = _displayWidth / _touchlessMgr.CurrentCamera.CaptureWidth;

                    // Initialize the points used for placing the image
                    _destPoints = new Point[3];
                    _destPoints[0] = new Point();
                    _destPoints[1] = new Point();
                    _destPoints[2] = new Point();

                    // Calculate the image's diagonal length
                    _imageDiagonal = (float)Math.Sqrt(_imageWidth * _imageWidth + _imageHeight * _imageHeight);
                    // The angle from the lower-left corner to the upper right corner (from North)
                    _imageDiagonalAngle = (float)Math.Atan2(_imageWidth, _imageHeight);

                    //draw here
                    pictureBox1.Show();
                }
                else
                {
                    MessageBox.Show("Need 2 markers");
                    buttonNewsPaperDemo_Click(this, e);
                }
            }
            else
            {

                newspaperDemo = false;
                labelDemoName.Text = "WUW";
                buttonNewsPaperDemo.Text = "NewsPaper";
                elementHostMenu.Hide();
                Cursor = Cursors.Arrow;
                labelDemoInstructions.Enabled = false;
                labelDemoInstructions.Text = "";
                ResetEnvironment();
            }

        }

        private void recalculateTransformation()
        {
            // Make sure the other two points are valid
            if (_destPoints[2].IsEmpty || _destPoints[1].IsEmpty)
                return;

            // Make local copies of the other two points
            Point upperRight = _destPoints[1];
            Point lowerLeft = _destPoints[2];

            // Determine the image scale based on the distance between the points and the base diagonal
            int dx = upperRight.X - lowerLeft.X;
            int dy = lowerLeft.Y - upperRight.Y;
            float scaledDiagonal = (float)Math.Sqrt(dx * dx + dy * dy);
            _imageScale = scaledDiagonal / _imageDiagonal;

            // Find the scaled height
            float scaledHeight = _imageHeight * _imageScale;

            // Find the current diagonal angle (from East)
            float currDiagAngle = (float)Math.Atan2(dy, dx);

            // Find the current left edge angle (from West)
            float currLeftEdgeAngle = (float)Math.PI - (currDiagAngle + _imageDiagonalAngle);

            // Find the x difference from the lower-left to the upper-left
            float diffX = (float)(Math.Cos(currLeftEdgeAngle) * scaledHeight);
            float diffY = (float)(Math.Sin(currLeftEdgeAngle) * scaledHeight);

            // Find the upper-left point
            _destPoints[0].X = (int)(lowerLeft.X - diffX);
            _destPoints[0].Y = (int)(lowerLeft.Y - diffY);

            updateImage();
        }

        private void updateImageM(object sender, MarkerEventArgs args)
        {
            // Set the lower-left point
            _destPoints[2].X = (int)(args.EventData.X * _displayScale);
            _destPoints[2].Y = (int)(args.EventData.Y * _displayScale);

            // Recalculate the upper-left point
            recalculateTransformation();
        }

        private void updateImageO(object sender, MarkerEventArgs args)
        {
            // Set the upper-right point
            _destPoints[1].X = (int)(args.EventData.X * _displayScale);
            _destPoints[1].Y = (int)(args.EventData.Y * _displayScale);

            // Recalculate the upper-left point
            recalculateTransformation();
        }

        private void updateImage()
        {
            Graphics graphics = pictureBox1.CreateGraphics();
            graphics.FillRectangle(Brushes.Black, 0, 0, 1024, 768);
            graphics.DrawImage(_image, _destPoints);
        }

        #endregion NewsPaper Demo

        #region Mail Demo

        private void buttonMailDemo_Click(object sender, EventArgs e)
        {
            if (mailDemo == false)
            {
                StopOtherApps(this, e);
                //elementHostMenu.Child = Control_clock;
                mailDemo = true;
                labelDemoName.Text = "Mail";
                buttonMailDemo.Text = "Stop Mail";
                elementHostMenu.Show();
                labelDemoInstructions.Enabled = true;
                labelDemoInstructions.Text = "Mail Demo Instructions:\n\n"
                    + "Check Mails.\n";
            }
            else
            {
                mailDemo = false;
                labelDemoName.Text = "WUW";
                buttonMailDemo.Text = "Mail";
                elementHostMenu.Hide();
                Cursor = Cursors.Arrow;
                labelDemoInstructions.Enabled = false;
                labelDemoInstructions.Text = "";
                ResetEnvironment();
            }
        }

        #endregion Mail Demo

        #region Jeff Han //liyanchang

        private void buttonJeffDemo_Click(object sender, EventArgs e)
        {
            if (jeffDemo == false)
            {
                StopOtherApps(this, e);
                jeffDemo = true;
                labelDemoName.Text = "Jeff Han";
                buttonJeffDemo.Text = "Stop Jeff";

                labelDemoInstructions.Enabled = true;
                labelDemoInstructions.Text = "Jeff Han Demo Instructions:\n\n"
                    + "Draws with all four fingers";
                btnShowHide_Hover();//liyanchang
                jeffHanPictureBox.SuspendLayout();
                jeffHanPictureBox.ClientSize = new System.Drawing.Size(1024, 768);
                jeffHanPictureBox.BackColor = Color.Black;
                jeffHanPictureBox.ResumeLayout(false);
                jeffHanPictureBox.Show();

                updatelabelMLocation(new Point(35, 9));
                updatelabelNLocation(new Point(35, 35));
                updatelabelOLocation(new Point(9, 9));
                updatelabelPLocation(new Point(9, 35));

            }
            else
            {
                jeffDemo = false;
                labelDemoName.Text = "WUW";
                buttonJeffDemo.Text = "Jeff Han";
                elementHostDraw.Hide();
                Cursor = Cursors.Arrow;
                labelDemoInstructions.Enabled = false;
                labelDemoInstructions.Text = "";
                jeffHanPictureBox.Hide();
                ResetEnvironment();
            }
        }

        int[,] mouseData = new int[4,2]{{0,0},{0,0},{0,0},{0,0}};
        SolidBrush[] brushData = new SolidBrush[4]{new SolidBrush(Color.Red),new SolidBrush(Color.Blue),new SolidBrush(Color.Green),new SolidBrush(Color.Yellow)};
        int dirX = new int();
        int dirY = new int();
        int magX = new int();
        int magY = new int();

        private void PainterForm_MouseMove(int index, int newX, int newY)
        {

            magX = Math.Min(100, 5 * (int)Math.Pow(Math.Abs(newX - mouseData[index, 0]), .5));
            magY = Math.Min(100, 5 * (int)Math.Pow(Math.Abs(newY - mouseData[index, 1]), .5));

            dirX = -1 * Math.Sign(newX - mouseData[index, 0]);
            dirY = -1 * Math.Sign(newX - mouseData[index, 1]);

            //make the cleaner. key to making the fade.
            SolidBrush cleanBrush = new SolidBrush(Color.FromArgb(25, 0, 0, 0));

            //LinearGradientBrush gradBrush = new
            //LinearGradientBrush(new Rectangle(0, 0, 1024, 768), Color.FromArgb(10, Color.Black), Color.FromArgb(10, Color.Black), 90, false);

            Graphics graphics = jeffHanPictureBox.CreateGraphics();

            //lets draw some circles.
            // need to offset
            //graphics.FillEllipse(brushData[index], newX+50, newY+50, 15, 15);

            //create random generator for randomness
           
            Random randomGenerator = new Random();
            for (int i = 0; i < 20; i++)
            {
                graphics.FillRectangle(brushData[index], newX + dirX * randomGenerator.Next(0, magX), newY + dirY * randomGenerator.Next(0, magY), 50, 50);
            }
            

            if (index == 0)
            {
                graphics.FillRectangle(cleanBrush, 0, 0, 1024, 768);
            }
            //clean up
            graphics.Dispose();

            //get ready for next time.
            mouseData[index, 0] = newX;
            mouseData[index, 1] = newY;
        }

        #endregion Jeff Han

        #region Fly Demo 
        //liyanchang

        private void buttonFlyDemo_Click(object sender, EventArgs e)
        {
            if (flyDemo == false)
            {
                StopOtherApps(this, e);
                flyDemo = true;
                labelDemoName.Text = "Fly Demo";
                buttonFlyDemo.Text = "Stop Fly";

                labelDemoInstructions.Enabled = true;
                labelDemoInstructions.Text = "Fly Demo Instructions:\n\n"
                    + "Catch the Fly";
                btnShowHide_Hover();//liyanchang
                flyPictureBox.Show();
            }
            else
            {
                flyDemo = false;
                labelDemoName.Text = "WUW";
                buttonFlyDemo.Text = "Fly Demo";
                elementHostDraw.Hide();
                Cursor = Cursors.Arrow;
                labelDemoInstructions.Enabled = false;
                labelDemoInstructions.Text = "";
                ResetEnvironment();
                flyPictureBox.Hide();
            }
        }

        // {x,y} {dirX, dirY}
        int[,] flyData = new int[2,2] {{ 500, 300 }, {0,0}};
        Random randomFly = new Random();
        // counter, random. when random is reached by counter. new random.
        int[] lastChange = new int[2]{1,1};

        private void flyDemo_MouseMove(Point leftThumb, Point rightThumb)
        {

            Graphics graphics = flyPictureBox.CreateGraphics();

            //catching action
            if (calculateDistance(leftThumb, rightThumb) < 30)
            {
                if (calculateDistance(leftThumb.X, leftThumb.Y, flyData[0, 0], flyData[0, 1]) < 100)
                {
                    //good job
                    graphics.FillEllipse(new SolidBrush(Color.Red), flyData[0, 0], flyData[0, 1], 30, 30);
                    //graphics.FillEllipse(new SolidBrush(Color.Red), leftThumb.X, leftThumb.Y, 100, 100);
                }
                else
                {   
                    //oops.
                    //graphics.FillEllipse(new SolidBrush(Color.LightBlue), flyData[0, 0], flyData[0, 1], 30, 30);
                    graphics.FillEllipse(new SolidBrush(Color.LightBlue), leftThumb.X, leftThumb.Y, 100, 100);
                }
            }

            //avoid leaving the screen.
            if (flyData[0, 0] < 100) { flyData[1, 0] = randomFly.Next(0, 10); }
            if (flyData[0, 1] < 100) { flyData[1, 1] = randomFly.Next(0, 10); }
            if (flyData[0, 0] > 1000) { flyData[1, 0] = randomFly.Next(-10, 0); }
            if (flyData[0, 1] > 700) { flyData[1, 1] = randomFly.Next(-10, 0); }

            if (lastChange[0] >= lastChange[1])
            {
                //new random.
                lastChange[1] = randomFly.Next(5, 30);
                //set new direction
                flyData[1, 0] = randomFly.Next(-10, 10);
                flyData[1, 1] = randomFly.Next(-10, 10);
                //reset count
                lastChange[0] = 0;
            }
            else
            {
                lastChange[0]++;
            }
           
            //Set the location of the fly
            flyData[0, 0] += flyData[1, 0] + randomFly.Next(-10,10) ;
            flyData[0, 1] += flyData[1, 1] + randomFly.Next(-10, 10);

            flyData[0, 0] = Math.Max(0, Math.Min(1000, flyData[0, 0]));
            flyData[0, 1] = Math.Max( 0, Math.Min( 768, flyData[0, 1]));

            //make the cleaner. key to making the fade.
            SolidBrush cleanBrush = new SolidBrush(Color.FromArgb(15, 0, 0, 0));
            //the fly brush
            SolidBrush flyBrush = new SolidBrush(Color.FromArgb(70, 255, 255, 255));

            //plot the fly
            graphics.FillEllipse(flyBrush, flyData[0,0], flyData[0,1], 15, 15);
            //graphics.FillEllipse(flyBrush, 100, 100, 15, 15);

            //fade things.
            graphics.FillRectangle(cleanBrush, 0, 0, 1024, 768);

            //clean up
            graphics.Dispose();

        }

        #endregion Fly Demo

        #region Menu Demo

        private void buttonMenuDemo_Click(object sender, EventArgs e)
        {
            if ( menuDemo == false)
            {
                StopOtherApps(this, e);
                //elementHostMenu.Child = Control_menu;
                menuDemo = true;
                labelDemoName.Text = "Menu";
                buttonMenuDemo.Text = "Stop Menu";
                elementHostMenu.Show();
                labelDemoInstructions.Enabled = true;
                labelDemoInstructions.Text = "Menu Demo Instructions:\n\n"
                    + "Clock follows Marker M\n";
            }
            else
            {
                menuDemo = false;
                labelDemoName.Text = "WUW";
                buttonMenuDemo.Text = "Menu";
                elementHostMenu.Hide();
                Cursor = Cursors.Arrow;
                labelDemoInstructions.Enabled = false;
                labelDemoInstructions.Text = "";
                ResetEnvironment();
            }
        }
        #endregion Menu Demo

        #region Gesture Demo

        private void buttonGestureDemo_Click(object sender, EventArgs e)
        {
            StopOtherApps(this, e);
        }

        #endregion Clock Demo

        //TAGGING
        #region 3DDemo
        
        private void button3DDemo_Click(object sender, EventArgs e)
        {

            if ( threedDemo == false)
            {
                StopOtherApps(this, e);
                threedDemo = true;
                labelDemoName.Text = "3D";
                button3DDemo.Text = "Stop 3D";
                labelDemoInstructions.Enabled = true;
                labelDemoInstructions.Text = "3d Demo Instructions:\n\n";

                lblResult.Hide();

                //initialize nyar components.
                NyARParam ap = new NyARParam();
                ap.loadARParamFromFile(AR_CAMERA_FILE);
                ap.changeScreenSize(SCREEN_WIDTH, SCREEN_HEIGHT);
                _raster = new DsBGRX32Raster(SCREEN_WIDTH, SCREEN_HEIGHT, SCREEN_WIDTH * 32 / 8);
                _utils = new NyARD3dUtil();

                // For each pattern
                NyARCode code1 = new NyARCode(16, 16);
                code1.loadARPattFromFile(AR_CODE_FILE1);
                _ar1 = new NyARSingleDetectMarker(ap, code1, 80.0);
                _ar1.setContinueMode(false);

                NyARCode code2 = new NyARCode(16, 16);
                code2.loadARPattFromFile(AR_CODE_FILE2);
                _ar2 = new NyARSingleDetectMarker(ap, code2, 80.0);
                _ar2.setContinueMode(false);

                NyARCode code3 = new NyARCode(16, 16);
                code3.loadARPattFromFile(AR_CODE_FILE3);
                _ar3 = new NyARSingleDetectMarker(ap, code3, 80.0);
                _ar3.setContinueMode(false);

            }

            else
            {
                threedDemo = false;
                labelDemoName.Text = "WUW";
                button3DDemo.Text = "Menu";
                Cursor = Cursors.Arrow;
                labelDemoInstructions.Enabled = false;
                labelDemoInstructions.Text = "";
                ResetEnvironment();

                lblResult.Show();
            }
        }

            private void nyar()
            {
                // - load the image to a bitmap
                Bitmap _latestFrameBitmap = (Bitmap)_latestFrame;

                // - create a new bitmap with diff. file format. PixelFormat.Format32bppArbg
                Bitmap _latestFrameShift = new Bitmap(_latestFrameBitmap.Width, _latestFrameBitmap.Height, PixelFormat.Format32bppArgb);
                _latestFrameShift.SetResolution(_latestFrameBitmap.HorizontalResolution, _latestFrameBitmap.VerticalResolution);

                // - copy the data from first bitmap to second.
                Graphics g = Graphics.FromImage(_latestFrameShift);
                g.DrawImage(_latestFrameBitmap, 0, 0);
                g.Dispose();

                // - change the bitmap into an intptr
                Rectangle _latestFrameShiftRect = new Rectangle(0, 0, _latestFrameShift.Width, _latestFrameShift.Height);
                BitmapData _latestFrameShiftData = _latestFrameShift.LockBits(_latestFrameShiftRect, ImageLockMode.ReadWrite, _latestFrameShift.PixelFormat);
                IntPtr fakeBuffer = _latestFrameShiftData.Scan0;

                _latestFrameShift.UnlockBits(_latestFrameShiftData);

                // - use the fake buffer
                _raster.setBuffer(fakeBuffer);

                //Begin to DETECT.

                //Try all three.
                _ar1.detectMarkerLite(_raster, 110);
                _ar2.detectMarkerLite(_raster, 110);
                _ar3.detectMarkerLite(_raster, 110);

                NyARSingleDetectMarker[] _arArray = new NyARSingleDetectMarker[3] { _ar1, _ar2, _ar3 };
                _arFinal = largestNyar(_arArray);

                is_marker_enable = _arFinal.detectMarkerLite(_raster, 110);

                if (is_marker_enable && _arFinal.getConfidence() > 0.3)
                {
                    labelDemoName.Text = "Pattern #" + largestNyarIndex(_arArray) + "[" + _arFinal.getConfidence().ToString() + "]";
                }
                else
                {
                    labelDemoName.Text = "No Pattern";
                }
                
               

                //NyARSingleDetectMarker[] _arArray = new NyARSingleDetectMarker[3]{_ar1, _ar2, _ar3};
                //_arFinal = largestNyar(_arArray);

                //is_marker_enable = _arFinal.detectMarkerLite(_raster, 110);

                //if (is_marker_enable)
                //{
                //    Microsoft.DirectX.Matrix trans_matrix = this.__MainLoop_trans_matrix;
                //    NyARTransMatResult trans_result = this.__MainLoop_nyar_transmat;
                //    _ar.getTransmationMatrix(trans_result);
                //    _utils.toD3dMatrix(trans_result, ref trans_matrix);
                //    float markerX = trans_matrix.M14;
                //    float markerDir = _ar.getDirection();
                //}

                // - do some action to let me know its working
                //if (_arFinal.getConfidence() > 0.4)
                //{
                //    labelDemoName.Text = "Pattern #" + largestNyarIndex(_arArray) + "[" + _arFinal.getConfidence().ToString() + "]";
                //}
                //else
                //{
                //    labelDemoName.Text = "No Pattern";
                //}

                //changing intPtr back into bitmap for kicks. (or testing)
                // Seems to cause memory issues.
                //Bitmap _latestFrameShiftDataBitmap = new Bitmap(_latestFrameShiftData.Width, _latestFrameShiftData.Height, _latestFrameShiftData.Stride, _latestFrameShiftData.PixelFormat, fakeBuffer);

                //display some feedback.
                pictureBoxAlbum.Show();
                pictureBoxAlbum.Image = _latestFrameShift;
            }

            private NyARSingleDetectMarker largestNyar(NyARSingleDetectMarker[] nyarArray)
            {
                NyARSingleDetectMarker maxNyar = nyarArray[0];
                double maxConfidence = 0;
                for (int i = 0; i < nyarArray.Length; i++)
                {
                    if (nyarArray[i].getConfidence() > maxConfidence)
                    {
                        maxConfidence = nyarArray[i].getConfidence();
                        maxNyar = nyarArray[i];
                    }
                }
                return maxNyar;
            }

            private int largestNyarIndex(NyARSingleDetectMarker[] nyarArray)
            {
                int maxIndex = 0;
                double maxConfidence = 0;
                for (int i = 0; i < nyarArray.Length; i++)
                {
                    if (nyarArray[i].getConfidence() > maxConfidence)
                    {
                        maxConfidence = nyarArray[i].getConfidence();
                        maxIndex = i;
                    }
                }
                return maxIndex;
            }

        #endregion 3DDemo
        //END TAGGING

        #endregion Demo Mode




    }
}

       		

        
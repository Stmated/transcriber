/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

// This code is just to show a couple of ways you could use the 
// GenericSampleSourceFilter.  For a more details discussion, check out the
// readme.txt

// Note that in order to use the MP3 methods, you must download and
/// install Bass.Net (which must be added as a reference) and BASS.DLL
/// from http://www.un4seen.com
//#define USING_BASS_DLL

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DirectShowLib;

#if USING_BASS_DLL
using Un4seen.Bass;
#endif

namespace transcriber_winform.GSSF
{
    /// <summary>
    /// A class to construct a graph using the GenericSampleSourceFilter.
    /// </summary>
    internal class DxPlay : IDisposable
    {
        #region Member variables

        // Event called when the graph stops
        public event EventHandler Completed = null;

        /// <summary>
        /// The class that retrieves the images
        /// </summary>
        private AbstractImageHandler m_ImageHandler;

        /// <summary>
        /// graph builder interfaces
        /// </summary>
        private IFilterGraph2 m_FilterGraph;

        /// <summary>
        /// Another graph builder interface
        /// </summary>
        private IMediaControl m_mediaCtrl;

#if DEBUG
        /// <summary>
        /// Allow you to "Connect to remote graph" from GraphEdit
        /// </summary>
        DsROTEntry m_DsRot;
#endif

        #endregion

        /// <summary>
        /// Release everything
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.CloseInterfaces();
        }

        /// <summary>
        /// Alternate cleanup
        /// </summary>
        ~DxPlay()
        {
            this.CloseInterfaces();
        }

        /// <summary>
        /// Play a video into a window using the GenericSampleSourceFilter as the video source
        /// </summary>
        /// <param name="sPath">Path for the ImageFromFiles class (if that's what we are using)
        /// to use to find images</param>
        /// <param name="hWin">Window to play the video in</param>
        public DxPlay(string sPath, Control hWin)
        {
            try
            {
                // pick one of our image providers
                //m_ImageHandler = new ImageFromFiles(sPath, 8);
                this.m_ImageHandler = new ImageFromPixels(20);
                //m_ImageHandler = new ImageFromMpg(@"c:\c1.mpg");
                //m_ImageHandler = new ImageFromMP3(@"c:\vss\media\track3.mp3");

                // Set up the graph
                this.SetupGraph(hWin);
            }
            catch
            {
                this.Dispose();
                throw;
            }
        }


        /// <summary>
        /// Start playing
        /// </summary>
        public void Start()
        {
            // Create a new thread to process events
            Thread t;
            t = new Thread(this.EventWait);
            t.Name = "Media Event Thread";
            t.Start();

            int hr = this.m_mediaCtrl.Run();
            DsError.ThrowExceptionForHR( hr );
        }

        /// <summary>
        /// Stop the capture graph.
        /// </summary>
        public void Stop()
        {
            int hr;

            hr = ((IMediaEventSink)this.m_FilterGraph).Notify(EventCode.UserAbort, IntPtr.Zero, IntPtr.Zero);
            DsError.ThrowExceptionForHR( hr );

            hr = this.m_mediaCtrl.Stop();
            DsError.ThrowExceptionForHR( hr );
        }


        /// <summary>
        /// Build the filter graph
        /// </summary>
        /// <param name="hWin">Window to draw into</param>
        private void SetupGraph(Control hWin)
        {
            // Get the graphbuilder object
            this.m_FilterGraph = new FilterGraph() as IFilterGraph2;

            // Get a ICaptureGraphBuilder2 to help build the graph
            var captureGraphBuilder = new CaptureGraphBuilder2() as ICaptureGraphBuilder2;

            try
            {
                // Link the ICaptureGraphBuilder2 to the IFilterGraph2
                var hr = captureGraphBuilder.SetFiltergraph(this.m_FilterGraph);
                DsError.ThrowExceptionForHR( hr );

#if DEBUG
                // Allows you to view the graph with GraphEdit File/Connect
                this.m_DsRot = new DsROTEntry(this.m_FilterGraph);
#endif

                // Our data source
                var source = new GenericSampleSourceFilter() as IBaseFilter;

                try
                {
                    // Get the pin from the filter so we can configure it
                    var ipin = DsFindPin.ByDirection(source, PinDirection.Output, 0);

                    // x264vfw
                    // {D76E2820-1563-11CF-AC98-00AA004C0FA9}
                    // MediaType_Video
                    // Pins: input(0), output(0)

                    // DirectVobSub
                    // {93A22E7A-5091-45EF-BA61-6DA26156A5D0}
                    // Pins:
                    //      Input - Name: Video, Id: In
                    //      Input - Name: Input, id: Input, 
                    //                  majortype: MEDIATYPE_Subtitle {E487EB08-6B26-4BE9-9DD3-993434D313FD}
                    //                  subtype: MEDIASUBTYPE_ASS {326444F7-686F-47FF-A4B2-C8C96307B4C2}
                    //                  formattype: FORMAT_SubtitleInfo {A33D2F7D-96BC-4337-B23B-A8B9FBC295E9}
                    //      Output - Name: Output, Id: Out
                    // Interfaces: IAMStreamSelect, IBaseFilter, IMediaFilter, IPersist, ISpecifyPropertyPages, IUnknown

                    // IMemInputPin
                    // {56A8689D-0AD4-11CE-B03A-0020AF0BA770}

                    try
                    {
                        // Configure the pin using the provided BitmapInfo
                        //this.ConfigurePusher(ipin as IGenericSampleConfig);

                        

                        var genericSampleConfig = ipin as IGenericSampleConfig;
                        this.m_ImageHandler.SetMediaType(genericSampleConfig);

                        // Specify the callback routine to call with each sample
                        hr = genericSampleConfig.SetBitmapCB(this.m_ImageHandler);
                        DsError.ThrowExceptionForHR(hr);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(ipin);
                    }

                    // Add the filter to the graph
                    hr = this.m_FilterGraph.AddFilter(source, "GenericSampleSourceFilter");
                    Marshal.ThrowExceptionForHR(hr);

                    // IFileSourceFilter
                    // {56A868A6-0AD4-11CE-B03A-0020AF0BA770}

                    // Connect the filters together, use the default renderer
                    hr = captureGraphBuilder.RenderStream(null, null, source, null, null);
                    // DsError.ThrowExceptionForHR( hr );  // Ignore any error.  Blindly assume there is no video

                    hr = captureGraphBuilder.RenderStream(null, MediaType.Audio, source, null, null);
                    // DsError.ThrowExceptionForHR( hr ); // Ignore any error.  Blindly assume there is no audio
                }
                finally
                {
                    Marshal.ReleaseComObject(source);
                }

                // Configure the Video Window
                var videoWindow = this.m_FilterGraph as IVideoWindow;
                ConfigureVideoWindow(videoWindow, hWin);

                // Grab some other interfaces
                this.m_mediaCtrl = this.m_FilterGraph as IMediaControl;
            }
            finally
            {
                Marshal.ReleaseComObject(captureGraphBuilder);
            }
        }

        /// <summary>
        /// Configure the video window
        /// </summary>
        /// <param name="videoWindow">Interface of the video renderer</param>
        /// <param name="hWin">Handle of the window to draw into</param>
        private static void ConfigureVideoWindow(IVideoWindow videoWindow, Control hWin)
        {
            // Set the output window
            var hr = videoWindow.put_Owner(hWin.Handle);
            if (hr >= 0) // If there is video
            {
                // Set the window style
                hr = videoWindow.put_WindowStyle((WindowStyle.Child | WindowStyle.ClipChildren | WindowStyle.ClipSiblings));
                DsError.ThrowExceptionForHR(hr);

                // Make the window visible
                hr = videoWindow.put_Visible(OABool.True);
                DsError.ThrowExceptionForHR(hr);

                // Position the playing location
                Rectangle rc = hWin.ClientRectangle;
                hr = videoWindow.SetWindowPosition(0, 0, rc.Right, rc.Bottom);
                DsError.ThrowExceptionForHR(hr);
            }
        }

        /// <summary>
        /// Shut down graph
        /// </summary>
        private void CloseInterfaces()
        {
            lock (this)
            {
                // Stop the graph
                if( this.m_mediaCtrl != null )
                {
                    // Stop the graph
                    this.m_mediaCtrl.Stop();
                    this.m_mediaCtrl = null;
                }

                if (this.m_ImageHandler != null)
                {
                    this.m_ImageHandler.Dispose();
                    this.m_ImageHandler = null;
                }

#if DEBUG
                if (this.m_DsRot != null)
                {
                    this.m_DsRot.Dispose();
                    this.m_DsRot = null;
                }
#endif

                // Release the graph
                if (this.m_FilterGraph != null)
                {
                    (this.m_FilterGraph as IMediaEventSink).Notify(EventCode.UserAbort, IntPtr.Zero, IntPtr.Zero);

                    Marshal.ReleaseComObject(this.m_FilterGraph);
                    this.m_FilterGraph = null;
                }
            }
            GC.Collect();
        }

        /// <summary>
        /// Called on a new thread to process events from the graph. The thread exits when the graph finishes.
        /// </summary>
        private void EventWait()
        {
            // Returned when GetEvent is called but there are no events
            const int E_ABORT = unchecked((int)0x80004004);

            int hr;
            IntPtr p1, p2;
            EventCode ec;
            EventCode exitCode = 0;

            IMediaEvent pEvent = (IMediaEvent)this.m_FilterGraph;

            do
            {
                // Read the event
                for (hr = pEvent.GetEvent(out ec, out p1, out p2, 100); hr >= 0; hr = pEvent.GetEvent(out ec, out p1, out p2, 100))
                {
                    Debug.WriteLine(ec);
                    switch(ec)
                    {
                            // If the clip is finished playing
                        case EventCode.Complete:
                        case EventCode.ErrorAbort:
                        case EventCode.UserAbort:
                            exitCode = ec;

                            // Release any resources the message allocated
                            hr = pEvent.FreeEventParams(ec, p1, p2);
                            DsError.ThrowExceptionForHR(hr);
                            break;

                        default:
                            // Release any resources the message allocated
                            hr = pEvent.FreeEventParams(ec, p1, p2);
                            DsError.ThrowExceptionForHR(hr);
                            break;
                    }
                }

                // If the error that exited the loop wasn't due to running out of events
                if (hr != E_ABORT)
                {
                    DsError.ThrowExceptionForHR(hr);
                }
            } while (exitCode == 0);

            // Send an event saying we are complete
            if (this.Completed != null)
            {
                CompletedArgs ca = new CompletedArgs(exitCode);
                this.Completed(this, ca);
            }

        }

        public class CompletedArgs : EventArgs
        {
            /// <summary>The result of the rendering</summary>
            /// <remarks>
            /// This code will be a member of DirectShowLib.EventCode.  Typically it 
            /// will be EventCode.Complete, EventCode.ErrorAbort or EventCode.UserAbort.
            /// </remarks>
            public EventCode Result;

            /// <summary>
            /// Used to construct an instace of the class.
            /// </summary>
            /// <param name="ec"></param>
            internal CompletedArgs(EventCode ec)
            {
                this.Result = ec;
            }
        }

    }

    // A generic class to support easily changing between my different sources of data.

    // Note: You DON'T have to use this class, or anything like it.  The key is the SampleCallback
    // routine.  How/where you get your bitmaps is ENTIRELY up to you.  Having SampleCallback call
    // members of this class was just the approach I used to isolate the data handling.
    abstract internal class AbstractImageHandler : IDisposable, IGenericSampleCB
    {
        #region Definitions

        /// <summary>
        /// 100 ns - used by a number of DS methods
        /// </summary>
        protected const long UNIT = 10000000;

        #endregion

        /// <summary>
        /// Number of callbacks that returned a positive result
        /// </summary>
        protected int m_iFrameNumber = 0;

        virtual public void Dispose()
        {
        }

        abstract public void SetMediaType(IGenericSampleConfig psc);

        abstract public int GetImage(int iFrameNumber, IntPtr ip, int iSize, out int iRead);

        virtual public int SetTimeStamps(IMediaSample pSample)
        {
            return 0;
        }

        /// <summary>
        /// Called by the GenericSampleSourceFilter.  This routine populates the MediaSample.
        /// </summary>
        /// <param name="pSample">Pointer to a sample</param>
        /// <returns>0 = success, 1 = end of stream, negative values for errors</returns>
        virtual public int SampleCallback(IMediaSample pSample)
        {
            int hr;
            IntPtr pData;

            try
            {
                // Get the buffer into which we will copy the data
                hr = pSample.GetPointer(out pData);
                if (hr >= 0)
                {
                    // Set TRUE on every sample for uncompressed frames
                    hr = pSample.SetSyncPoint(true);
                    if (hr >= 0)
                    {
                        // Find out the amount of space in the buffer
                        int cbData = pSample.GetSize();

                        hr = this.SetTimeStamps(pSample);
                        if (hr >= 0)
                        {
                            int iRead;

                            // Get copy the data into the sample
                            hr = this.GetImage(this.m_iFrameNumber, pData, cbData, out iRead);
                            if (hr == 0) // 1 == End of stream
                            {
                                pSample.SetActualDataLength(iRead);

                                // increment the frame number for next time
                                this.m_iFrameNumber++;
                            }
                        }
                    }
                }
            }
            finally
            {
                // Release our pointer the the media sample.  THIS IS ESSENTIAL!  If
                // you don't do this, the graph will stop after about 2 samples.
                Marshal.ReleaseComObject(pSample);
            }

            return hr;
        }
    }

    /// <summary>
    /// Class to provide image data.  Note that the Bitmap class is very easy to use,
    /// but not terribly efficient.  If you aren't getting the performance you need,
    /// replacing that is a good place start.
    /// 
    /// Note that this class assumes that the images to show are all in the same
    /// directory, and are named 00000001.jpg, 00000002.jpg, etc
    /// 
    /// Also, make sure you read the comments on the ImageHandler class.
    /// </summary>
    internal class ImageFromFiles : AbstractImageHandler
    {
        [DllImport("Kernel32.dll", EntryPoint="RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        /// <summary>
        /// How many frames to show the bitmap in.  Using 1 will return a new
        /// image for each frame.  Setting it to 5 would show the same image
        /// in 5 frames, etc.  So, if you are running at 5 FPS, and you set DIV
        /// to 5, each image will show for 1 second.
        /// </summary>
        private const int DIV = 1;

        /// <summary>
        /// Contains the IntPtr to the raw data
        /// </summary>
        private BitmapData m_bmd;

        /// <summary>
        /// Needed to release the m_bmd member
        /// </summary>
        private Bitmap m_bmp;

        /// <summary>
        /// Path that contains the images
        /// </summary>
        private string m_sPath;

        // Number of frames per second
        private long m_FPS;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sPath">The directory that contains the images.</param>
        public ImageFromFiles(string sPath, long FPS)
        {
            this.m_sPath = sPath;
            this.m_FPS = UNIT / FPS;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        override public void Dispose()
        {
            // Release any outstanding bitmaps
            if (this.m_bmp != null)
            {
                this.m_bmp.UnlockBits(this.m_bmd);
                this.m_bmp = null;
                this.m_bmd = null;
            }
        }

        /// <summary>
        /// Set the Mediatype from a bitmap
        /// </summary>
        override public void SetMediaType(IGenericSampleConfig psc)
        {
            BitmapInfoHeader bmi = new BitmapInfoHeader();

            // Make sure we have an image to get the data from
            if (this.m_bmp == null)
            {
                int i;
                IntPtr ip = IntPtr.Zero;
                this.GetImage(0, ip, 0, out i);
            }

            // Build a BitmapInfo struct using the parms from the file
            bmi.Size = Marshal.SizeOf(typeof(BitmapInfoHeader));
            bmi.Width = this.m_bmd.Width;
            bmi.Height = this.m_bmd.Height * -1;
            bmi.Planes = 1;
            bmi.BitCount = 32;
            bmi.Compression = 0;
            bmi.ImageSize = (bmi.BitCount / 8) * bmi.Width * bmi.Height;
            bmi.XPelsPerMeter = 0;
            bmi.YPelsPerMeter = 0;
            bmi.ClrUsed = 0;
            bmi.ClrImportant = 0;

            int hr = psc.SetMediaTypeFromBitmap(bmi, this.m_FPS);
            DsError.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Populate the data buffer.  In this class I'm retrieving bitmaps 
        /// from files based on the current frame number.
        /// </summary>
        /// <param name="iFrameNumber">Frame number</param>
        /// <param name="ip">A pointer to the memory to populate with the bitmap data</param>
        /// <param name="iRead">returns the number of parameters read</param>
        /// <returns>0 on success and 1 on end of stream</returns>
        override public int GetImage(int iFrameNumber, IntPtr ip, int iSize, out int iRead)
        {
            int hr = 0;

            if (iFrameNumber % DIV == 0)
            {
                try
                {
                    // Open the next image
                    string sFileName = String.Format(@"{1}\{0:00000000}.jpg", iFrameNumber / DIV + 1, this.m_sPath);
                    Bitmap bmp = new Bitmap(sFileName);
                    Rectangle r = new Rectangle(0, 0, bmp.Width, bmp.Height);

                    // Release the previous image
                    if (this.m_bmd != null)
                    {
                        this.m_bmp.UnlockBits(this.m_bmd);
                        this.m_bmp.Dispose();
                    }

                    // Store the pointers
                    this.m_bmp = bmp;
                    this.m_bmd = this.m_bmp.LockBits(r, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                    // Only do the copy if we have a place to put the data
                    if (ip != IntPtr.Zero)
                    {
                        // Copy from the bmd to the MediaSample
                        CopyMemory(ip, this.m_bmd.Scan0, iSize);
                    }
                }
                catch
                {
                    // Presumably we ran out of files.  Terminate the stream
                    hr = 1;
                }
            }

            iRead = iSize;

            return hr;
        }

        /// <summary>
        /// Calculate the timestamps based on the frame number and the frames per second
        /// </summary>
        /// <param name="pSample"></param>
        /// <returns></returns>
        override public int SetTimeStamps(IMediaSample pSample)
        {
            // Calculate the start/end times based on the current frame number
            // and frame rate
            DsLong rtStart = new DsLong(m_iFrameNumber * this.m_FPS);
            DsLong rtStop  = new DsLong(rtStart + this.m_FPS);

            // Set the times into the sample
            int hr = pSample.SetTime(rtStart, rtStop);

            return hr;
        }
    }

    /// <summary>
    /// Alternate class to provide image data.  
    /// 
    /// This class just generates pretty colored bitmaps.
    /// 
    /// Also, make sure you read the comments on the ImageHandler class.
    /// </summary>
    internal class ImageFromPixels : AbstractImageHandler
    {
        // How many frames to return before returning End Of Stream
        private const int MAXFRAMES = 1000;

        /// <summary>
        /// How many frames to show the bitmap in.  Using 1 will return a new
        /// image for each frame.  Setting it to 5 would show the same image
        /// in 5 frames, etc.
        /// </summary>
        private const int DIV = 1;

        private const int HEIGHT = 240;
        private const int WIDTH = 320;
        private const int BPP = 32;

        // Used to make the pretty picture
        private int m_g = 0;
        private int m_b = 0;

        // Number of frames per second
        private long m_FPS;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="FPS">Frames per second to use</param>
        public ImageFromPixels(long FPS)
        {
            this.m_FPS = UNIT / FPS;
            this.m_b = 211;
            this.m_g = 197;
        }

        /// <summary>
        /// Set the media type on the IGenericSampleConfig
        /// </summary>
        override public void SetMediaType(IGenericSampleConfig psc)
        {
            BitmapInfoHeader bmi = new BitmapInfoHeader();

            // Build a BitmapInfo struct using the parms from the file
            bmi.Size = Marshal.SizeOf(typeof(BitmapInfoHeader));
            bmi.Width = WIDTH;
            bmi.Height = HEIGHT * -1;
            bmi.Planes = 1;
            bmi.BitCount = BPP;
            bmi.Compression = 0;
            bmi.ImageSize = (bmi.BitCount / 8) * bmi.Width * bmi.Height;
            bmi.XPelsPerMeter = 0;
            bmi.YPelsPerMeter = 0;
            bmi.ClrUsed = 0;
            bmi.ClrImportant = 0;

            int hr = psc.SetMediaTypeFromBitmap(bmi, this.m_FPS);
            DsError.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Populate the data buffer.  In this class I'm just generating bitmaps of random colors.
        /// 
        /// Using Marshal.Write* is *really* slow.  For decent performance, consider using pointers and unsafe code.
        /// </summary>
        /// <param name="iFrameNumber">Frame number</param>
        /// <param name="ip">A pointer to the memory to populate with the bitmap data</param>
        /// <returns>0 on success and 1 on end of stream</returns>
        unsafe override public int GetImage(int iFrameNumber, IntPtr ip, int iSize, out int iRead)
        {
            int hr = 0;

            if (iFrameNumber % DIV == 0)
            {
                if (iFrameNumber < MAXFRAMES)
                {
                    var c = Color.FromArgb(0, ((iFrameNumber * 2) % 255), ((iFrameNumber * 2 + this.m_g) % 255), ((iFrameNumber * 2 + this.m_b) % 255));

                    this.m_g += 3;
                    this.m_b += 7;

                    // Uncomment this line (and the one inside the loop), and comment out
                    // the Marshal.WriteInt32 to DRASTICALLY improve performance, particularly
                    // under the vs 2005 debugger.
                    int* bp = (int*)ip.ToPointer();

                    for (int x=0; x < (HEIGHT * WIDTH); x+=1)
                    {
                        *(bp + x) = c.ToArgb();

                        //Marshal.WriteInt32(ip, x * (BPP/8), c.ToArgb());
                    }
                }
                else
                {
                    hr = 1; // End of stream
                }
            }

            iRead = iSize;

            return hr;
        }

        /// <summary>
        /// Calculate the timestamps based on the frame number and the frames per second
        /// </summary>
        override public int SetTimeStamps(IMediaSample pSample)
        {
            // Calculate the start/end times based on the current frame number
            // and frame rate
            var rtStart = new DsLong(m_iFrameNumber * this.m_FPS);
            var rtStop  = new DsLong(rtStart + this.m_FPS);

            // Set the times into the sample
            var hr = pSample.SetTime(rtStart, rtStop);

            return hr;
        }
    }
}

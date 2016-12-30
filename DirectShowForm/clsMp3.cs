using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectShowLib;

namespace DirectShowForm
{  
    public enum GraphState
    {
        Stopped = FilterState.Stopped,
        Paused = FilterState.Paused,
        Playing = FilterState.Running,
    }

    class clsMp3
    {
        private static clsMp3 selfInstance = null;
        public static clsMp3 getInstance
        {
            get
            {
                if (selfInstance == null) selfInstance = new clsMp3();
                return selfInstance;
            }
        }
        
        private IGraphBuilder iGraphBuilder = null;
        private IMediaControl iMediaControl = null;
        private IMediaEventEx iMediaEventEx = null;
        private IBasicAudio   iBasicAudio = null;
        private IMediaSeeking iMediaSeeking = null;

        private bool bReady = false;
        private long duration = 0;

        public GraphState Status { get; private set; }


        public clsMp3()
        {
            iGraphBuilder = null;
            iMediaControl = null;
            iMediaEventEx = null;
            iBasicAudio = null;
            iMediaSeeking = null;
            bReady = false;
            duration = 0;
        }

        ~clsMp3()
        {
            Cleanup();
        }
        
        public void Cleanup()
        {
            if (iMediaControl != null) iMediaControl.Stop();
            if (iMediaControl != null) iMediaControl = null;
            if (iMediaEventEx != null) iMediaEventEx = null;
            if (iBasicAudio != null)   iBasicAudio = null;
            if (iMediaSeeking != null) iMediaSeeking = null;            
            if (iGraphBuilder != null) iGraphBuilder = null;
            bReady = false;
        }
        public bool Open(string strFile)
        {
            Cleanup();
            bReady = false;

            iGraphBuilder = (IGraphBuilder)new FilterGraph();
            if (iGraphBuilder == null) return false;

            iMediaControl = iGraphBuilder as IMediaControl;
            iMediaEventEx = iGraphBuilder as IMediaEventEx;
            iBasicAudio = iGraphBuilder as IBasicAudio;
            iMediaSeeking = iGraphBuilder as IMediaSeeking;
                        
            int nResult = iGraphBuilder.RenderFile(strFile, null);
            if (nResult < 0) return false;

            bReady = true;
            if (iMediaSeeking != null)
            {
                iMediaSeeking.SetTimeFormat(TimeFormat.MediaTime);
                iMediaSeeking.GetDuration(out duration);
            }
            return true;
        }
        
        public bool Play()
        {
            if (bReady && iMediaControl != null)
            {
                Status = GraphState.Playing;
                iMediaControl.Run();
                return true;
            }
            return false;
        }

        public bool Pause()
        {
            if (bReady && iMediaControl != null)
            {
                Status = GraphState.Paused;
                iMediaControl.Pause();
                return true;
            }
            return false;
        }

        public bool Stop()
        {
            if (bReady && iMediaControl != null)
            {
                Status = GraphState.Stopped;
                iMediaControl.Stop();
                return true;
            }
            return false;
        }

        public bool WaitForCompletion(int msTimeout, EventCode evCode)
        {
            if (bReady && iMediaEventEx != null)
            {
                int nResult = iMediaEventEx.WaitForCompletion(msTimeout, out evCode);
                return evCode > 0;
            }
            return false;
        }

        public bool SetVolume(int nVolume)
        {
            if (bReady && iBasicAudio != null)
            {
                iBasicAudio.put_Volume(nVolume);
                return true;
            }
            return false;
        }

        public int GetVolume()
        {
            if (bReady && iBasicAudio != null)
            {
                int nVolume = 0;
                iBasicAudio.get_Volume( out nVolume);
                return nVolume;
            }
            return 0;
        }
        
        public long GetRunningTime()
        {
            return (duration / 10000);//return duration;
        }

        public long GetPosition()
        {
            if (bReady && iMediaSeeking != null)
            {
                long curpos = 0;
                int nResult = iMediaSeeking.GetCurrentPosition( out curpos);
                return (curpos / 10000);//return curpos;
            }
            return 0;
        }

        public bool SetPositions(long pCurrent)
        {
            if (bReady && iMediaSeeking != null)
            {
                long pStop = -1;
                int result = iMediaSeeking.GetStopPosition(out pStop);
                result = iMediaSeeking.SetPositions( (pCurrent * 10000), AMSeekingSeekingFlags.AbsolutePositioning, pStop, AMSeekingSeekingFlags.AbsolutePositioning);
                return true;
            }
            return false;
        }

        public bool IsPlaying()
        {
            if (iMediaControl == null) return false;

            //song end
            if (GetRunningTime() == GetPosition()) return false;
            
            FilterState filterState;
            int result = iMediaControl.GetState(1000, out filterState);
            if (result == 0 || result == DsResults.S_StateIntermediate) return filterState == FilterState.Running;
            
            return false; 
        }

        public void SkipBack()
        {            
            if (bReady && iMediaSeeking != null)
            {
                long curpos = 0;
                int nResult = iMediaSeeking.GetCurrentPosition(out curpos);
                long pCurrent = (curpos - 10000000);                
                nResult = iMediaSeeking.SetPositions(pCurrent, AMSeekingSeekingFlags.AbsolutePositioning, 0, AMSeekingSeekingFlags.NoPositioning);
            }           
        }

        public void SkipForward()
        {
            if (bReady && iMediaSeeking != null)
            {
                long curpos = 0;
                int nResult = iMediaSeeking.GetCurrentPosition(out curpos);
                long pCurrent = (curpos + 10000000);
                nResult = iMediaSeeking.SetPositions(pCurrent, AMSeekingSeekingFlags.AbsolutePositioning, 0, AMSeekingSeekingFlags.NoPositioning);
            }
        }

    }
}

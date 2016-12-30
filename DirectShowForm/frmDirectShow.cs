using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;

namespace DirectShowForm
{
    public partial class frmDirectShow : Form
    {
        private DirectShowForm.clsMp3 dshowPlay = DirectShowForm.clsMp3.getInstance;

        private ArrayList m_AryFilelist = new ArrayList();
        private int m_nIndex = 0;
        private string m_strCurrentMp3Path = "";
        private string m_strPlayTime = "";
               

        public frmDirectShow()
        {
            InitializeComponent();
        }

        private void frmDirectShow_Load(object sender, EventArgs e)
        {
            MoveLocationDialog();
        }

        private void frmDirectShow_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Stop();
        }

        private void pbLeft_Click(object sender, EventArgs e)
        {
            PrePlayMusic();
        }       

        private void pbRight_Click(object sender, EventArgs e)
        {
            NextPlayMusic();
        }

        private void pbPlus_Click(object sender, EventArgs e)
        {
            int nVolume = 0;
            nVolume = dshowPlay.GetVolume();
            nVolume += 100;

            if (nVolume > 0)
                nVolume = 0;

            dshowPlay.SetVolume(nVolume);
        }

        private void pbMinus_Click(object sender, EventArgs e)
        {
            int nVolume = 0;
            nVolume = dshowPlay.GetVolume();
            nVolume -= 100;

            if (nVolume < -10000)
                nVolume = -10000;

            dshowPlay.SetVolume(nVolume);
        }

        private void pbPlay_Click(object sender, EventArgs e)
        {
            if( dshowPlay.Status == GraphState.Paused)
                dshowPlay.Play();
            else if (dshowPlay.Status == GraphState.Playing)
                dshowPlay.Pause();
        }

        private void pbAddfile_Click(object sender, EventArgs e)
        {
            m_AryFilelist.Clear();

            FolderBrowserDialog selectfolder = new FolderBrowserDialog();
            selectfolder.Description = "Select Mp3";
            selectfolder.SelectedPath = Application.StartupPath;
            if (selectfolder.ShowDialog() == DialogResult.OK)
            {
                if (selectfolder.SelectedPath != "")
                    SetFileInfo(selectfolder.SelectedPath, m_AryFilelist);
            }

            NextPlayMusic(true);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (dshowPlay.Status == GraphState.Paused) return;
            
            int nTotalTime = (int)(dshowPlay.GetRunningTime() / 1000);
            long nCurentPos = dshowPlay.GetPosition();
            int nCurentTime = (int)(nCurentPos / 1000);
            m_strPlayTime = string.Format("{0:D2}:{1:D2}/{2:D2}:{3:D2}", (nCurentTime / 60), (nCurentTime % 60), (nTotalTime / 60), (nTotalTime % 60));
            lblTime.Text = m_strPlayTime;

            trackBar1.Value = (int)(dshowPlay.GetPosition()); 

            if (!dshowPlay.IsPlaying())
            {
                timer1.Stop();
                NextPlayMusic();
            }            
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            dshowPlay.SetPositions((trackBar1.Value ));
        }

        private void pbClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MoveLocationDialog(bool bCenter = false)
        {
            if (bCenter)
            {
                int nScreenWidth = Screen.PrimaryScreen.Bounds.Width;
                int nScreenHeight = Screen.PrimaryScreen.Bounds.Height;
                int nWidht = (nScreenWidth - this.Width);
                int nHeight = (nScreenHeight - this.Height);
                this.Location = new Point((nWidht / 2), (nHeight / 2));
            }
            else
            {
                int nScreenWidth = Screen.PrimaryScreen.Bounds.Width;
                int nScreenHeight = Screen.PrimaryScreen.Bounds.Height;
                int cx = ClientSize.Width;
                int cy = ClientSize.Height;
                this.Location = new Point((nScreenWidth - cx), (nScreenHeight - cy) - 30);
            }
        }

        private string GetFileExtName(string strFilename)
        {
            int nPos = strFilename.LastIndexOf('.');
            int nLength = strFilename.Length;
            if (nPos < nLength)
                return strFilename.Substring(nPos + 1, (nLength - nPos) - 1);
            return string.Empty;
        }

        private string GetFileName(string strFilename)
        {
            int nPos = strFilename.LastIndexOf('\\');
            int nLength = strFilename.Length;
            if (nPos < nLength)
                return strFilename.Substring(nPos + 1, (nLength - nPos) - 1);
            return strFilename;
        }
        private void SetFileInfo(string strPath, ArrayList AryFilelist)
        {
            string[] strfileList = Directory.GetFiles(strPath);
            foreach (string strFileName in strfileList)
            {
                if (GetFileExtName(strFileName).ToLower() == "mp3")
                    AryFilelist.Add(strFileName);
            }

            string[] strDirList = Directory.GetDirectories(strPath);
            foreach (string strDir in strDirList)
                SetFileInfo(strDir, AryFilelist);
        }

        private void Stop()
        {
            dshowPlay.Stop();

            timer1.Stop();
        }

        private void Pause()
        {
            dshowPlay.Pause();
        }

        private void Play()
        {
            dshowPlay.Open(m_strCurrentMp3Path);

            lblMusicInfo.Text = ReadMp3TagInfo();

            timer1.Stop();
            timer1.Start();

            trackBar1.Visible = true;
            trackBar1.Maximum = (int)(dshowPlay.GetRunningTime());
            trackBar1.Minimum = 0;
            
            dshowPlay.Play();

            dshowPlay.SetVolume(0);
        }

        private string ReadMp3TagInfo()
        {
            string strMusicInfo = "";
            using (FileStream fs = File.OpenRead(m_strCurrentMp3Path))
            {
                try
                {
                    byte[] byID = new byte[3];         //  3
                    byte[] byTitle = new byte[30];     //  30
                    byte[] byArtist = new byte[30];    //  30 
                    byte[] byAlbum = new byte[30];     //  30 
                    byte[] byYear = new byte[4];       //  4 
                    byte[] byComment = new byte[30];   //  30 
                    byte[] byGenre = new byte[1];      //  1
                    fs.Seek(-128, SeekOrigin.End);
                    fs.Read(byID, 0, 3);
                    fs.Read(byTitle, 0, 30);
                    fs.Read(byArtist, 0, 30);
                    fs.Read(byAlbum, 0, 30);
                    fs.Read(byYear, 0, 4);
                    fs.Read(byComment, 0, 30);
                    fs.Read(byGenre, 0, 1);
                    string strID = Encoding.Default.GetString(byID);
                    if (strID.Equals("TAG"))
                    {
                        string strTitle = Encoding.Default.GetString(byTitle).Replace("\0", "");
                        string strArtist = Encoding.Default.GetString(byArtist).Replace("\0", "");
                        string strAlbum = Encoding.Default.GetString(byAlbum).Replace("\0", "");
                        string strYear = Encoding.Default.GetString(byYear).Replace("\0", "");
                        string strComment = Encoding.Default.GetString(byComment).Replace("\0", "");
                        string strGenre = Encoding.Default.GetString(byGenre).Replace("\0", "");

                        if (strArtist != "" && strTitle != "")
                            strMusicInfo = string.Format("{0} - {1}", strArtist, strTitle);
                    }
                }
                catch (Exception) { }
            }
            return strMusicInfo;
        }

        private void PrePlayMusic()
        {
            if (m_AryFilelist.Count == 0) return;

            m_nIndex--;

            if (m_nIndex < 0)
                m_nIndex = m_AryFilelist.Count - 1;

            m_strCurrentMp3Path = m_AryFilelist[m_nIndex].ToString();

            Stop();
            Play();
        }

        private void NextPlayMusic(bool bFirstPlay = false)
        {
            if (m_AryFilelist.Count == 0) return;

            if (bFirstPlay)
            {
                m_nIndex = 0;
                m_strCurrentMp3Path = m_AryFilelist[m_nIndex].ToString();

                Stop();
                Play();
            }
            else
            {
                m_nIndex++;

                if (m_nIndex > (m_AryFilelist.Count - 1))
                    m_nIndex = 0;

                m_strCurrentMp3Path = m_AryFilelist[m_nIndex].ToString();

                Stop();
                Play();
            }
        }


    }
}

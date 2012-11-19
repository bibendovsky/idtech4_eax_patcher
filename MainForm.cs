using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Id4Eax4Patcher
{
    public partial class MainForm : Form
    {
        GameListItem mGame;
        GameListItem[] mGameList;
        byte[] mBytes;
        FileStream mStream;
        int mPatchOffset;
        int mRevertOffset;

        string PATCH_STRING = "EAX4.0";
        string REVERT_STRING = "EAX2.0";
        readonly byte[] PATCH_BYTES;
        readonly byte[] REVERT_BYTES;


        public MainForm()
        {
            InitializeComponent();

            PATCH_BYTES = Encoding.ASCII.GetBytes(PATCH_STRING);
            REVERT_BYTES = Encoding.ASCII.GetBytes(REVERT_STRING);
        }

        GameType DetectGame()
        {
            foreach (var game in mGameList)
            {
                if (!string.IsNullOrWhiteSpace(game.ExeName) && File.Exists(game.ExeName))
                    return game.Type;
            }

            return GameType.None;
        }

        private bool LoadFile(string fileName)
        {
            try
            {
                mStream = new FileStream(
                    fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception ex)
            {
                errorProvider.SetError(
                    gameComboBox,
                    "Failed to open a file \"" + fileName + "\":\n" + ex.Message);
            }

            if (mStream == null)
                return false;

            if (mStream.Length > 16000000)
            {
                errorProvider.SetError(
                    gameComboBox, "File \"" + fileName + "\" is too big.");
                return false;
            }

            mBytes = new byte[mStream.Length];

            if (mBytes == null)
                throw new OutOfMemoryException();

            int readCount = mStream.Read(mBytes, 0, (int)mStream.Length);

            if (readCount != mStream.Length)
            {
                errorProvider.SetError(
                    gameComboBox, "Failed to read data from \"" + fileName + "\".");
                return false;
            }

            return true;
        }

        private int FindBytes(byte[] bytes)
        {
            if (bytes == null)
                throw new NullReferenceException("bytes");

            if (bytes.Length == 0)
                throw new ArgumentException("Empty array", "bytes");

            int i;
            int n;
            int index = 0;

            for (i = 0, n = mBytes.Length - bytes.Length; i < n && index != bytes.Length; ++i)
            {
                if (mBytes[i] == bytes[index])
                    ++index;
                else
                    index = 0;
            }

            if (index == bytes.Length)
                return i - bytes.Length;

            return 0;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            mGameList = new GameListItem[] {
                new GameListItem(GameType.None, "<Not found>", ""),
                new GameListItem(GameType.Doom3, "Doom 3", "doom3.exe"),
                new GameListItem(GameType.Prey, "Prey", "prey.exe"),
                new GameListItem(GameType.Quake4, "Quake 4", "quake4.exe")
            };

            if (mGameList == null)
                throw new OutOfMemoryException();

            var game = DetectGame();
            gameComboBox.DataSource = mGameList;
            gameComboBox.SelectedValue = game;
            mGame = (GameListItem)gameComboBox.SelectedItem;

            if (game != GameType.None)
            {
                if (LoadFile(mGame.ExeName))
                {
                    mPatchOffset = FindBytes(PATCH_BYTES);
                    mRevertOffset = FindBytes(REVERT_BYTES);

                    bool foundPatch = (mPatchOffset > 0);
                    bool foundRevert = (mRevertOffset > 0);

                    if (foundPatch && foundRevert)
                    {
                        errorProvider.SetError(
                            gameComboBox, "File \"" + mGame.ExeName + "\" contains both data to patch and to revert.\n" +
                            "Not original file?");
                    }
                    else if (!foundPatch && !foundRevert)
                    {
                        errorProvider.SetError(
                            gameComboBox, "File \"" + mGame.ExeName + "\" contains no data to patch/revert.");
                    }
                    else
                    {
                        if (foundPatch)
                            patchButton.Enabled = true;

                        if (foundRevert)
                            revertButton.Enabled = true;
                    }
                }
            }
            else
            {
                errorProvider.SetError(gameComboBox,
                    "Game not found. Please put the\nprogram into the game folder.");
            }
        }

        private void patchButton_Click(object sender, EventArgs e)
        {
            ((Button)sender).Enabled = false;

            Patch(REVERT_BYTES, mPatchOffset);
        }

        private void revertButton_Click(object sender, EventArgs e)
        {
            ((Button)sender).Enabled = false;

            Patch(PATCH_BYTES, mRevertOffset);
        }

        private bool Patch(byte[] patchData, int offset)
        {
            try
            {
                if (patchData == null)
                    throw new NullReferenceException("patchData");

                if (patchData.Length == 0)
                    throw new ArgumentException("Empty array.", "patchData");

                if (offset <= 0)
                    throw new ArgumentException("Non-positive patch offset.", "offset");

                mStream.Position = offset;
                mStream.Write(patchData, 0, patchData.Length);
                mStream.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            MessageBox.Show(this, "Success!", "idTech4 EAX Patcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;

            if (mStream != null)
            {
                mStream.Close();
                mStream = null;
            }
        }
    }
}

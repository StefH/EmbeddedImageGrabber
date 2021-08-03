using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace ImageGrabber
{
    public partial class ImageGrabberForm : Form
    {
        #region Data

        /// <summary>
        /// The assembly whose embedded images are currently being displayed.
        /// </summary>
        private Assembly currentAssembly;

        #endregion // Data

        #region Constructors

        public ImageGrabberForm()
        {
            InitializeComponent();
        }

        public ImageGrabberForm(string assemblyPath)
            : this()
        {
            if (assemblyPath != null)
                LoadImagesFromAssembly(assemblyPath);

            splitContainer.Panel2Collapsed = true;

            string[] modes = Enum.GetNames(typeof(PictureBoxSizeMode));
            toolStripComboBox.Items.AddRange(modes);
            toolStripComboBox.Items.Remove("AutoSize");
            toolStripComboBox.Text = "CenterImage";

            toolStripComboBox.Visible = false;
            toolStripComboSeparator.Visible = false;
        }

        #endregion // Constructors

        #region Event Handlers

        #region Drag-Drop

        #region DragEnter

        private void ImageGrabberForm_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] filePaths = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (filePaths.Length != 1)
                return;

            Assembly assembly = LoadAssembly(filePaths[0], false);
            if (assembly == null)
                return;

            e.Effect = DragDropEffects.Link;
        }

        #endregion // DragEnter

        #region DragDrop

        private void ImageGrabberForm_DragDrop(object sender, DragEventArgs e)
        {
            string assemblyPath = (e.Data.GetData(DataFormats.FileDrop) as string[])[0];
            LoadImagesFromAssembly(assemblyPath);
        }

        #endregion // DragDrop

        #endregion // Drag-Drop

        #region ToolStrip Commands

        #region Open

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            PerformOpen();
        }

        #endregion // Open

        #region Save

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            PerformSave();
        }

        private void saveAllToolStripButton_Click(object sender, EventArgs e)
        {
            PerformSaveAll();
        }

        #endregion // Save

        #region Copy

        private void copyToolStripButton_Click(object sender, EventArgs e)
        {
            PerformCopy();
        }

        #endregion // Copy

        #region View/Hide Properties

        private void propertiesToolStripButton_Click(object sender, EventArgs e)
        {
            PerformViewHideProperties();
        }

        #endregion // View/Hide Properties

        #region SizeMode

        private void toolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Created)
                pictureBox.SizeMode = (PictureBoxSizeMode)Enum.Parse(typeof(PictureBoxSizeMode), toolStripComboBox.Text);
        }

        #endregion // SizeMode

        #endregion // ToolStrip Commands

        #region BindingSource

        #region ListChanged

        private void bindingSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.Reset)
            {
                bindingSource.Position = 0;

                bool imagesExist = bindingSource.Count > 0;

                copyToolStripButton.Enabled = imagesExist;
                saveToolStripButton.Enabled = imagesExist;
                saveAllToolStripButton.Enabled = imagesExist;
                toolStripComboBox.Enabled = imagesExist;
                propertiesToolStripButton.Enabled = imagesExist;
                tabControl.Enabled = imagesExist;
                propertyGrid.Enabled = imagesExist;

                if (!imagesExist)
                {
                    MessageBox.Show(
                        "The selected assembly does not have any embedded images.",
                        "No Images",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                if (propertyGrid.DataBindings.Count == 0)
                {
                    pictureBox.DataBindings.Add("Image", bindingSource, "Image");
                    propertyGrid.DataBindings.Add("SelectedObject", bindingSource, "ResourceDetails");

                    dataGridView.DataSource = bindingSource;
                    dataGridView.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dataGridView.Columns[1].Visible = false;
                }
            }
        }

        #endregion // ListChanged

        #endregion // BindingSource

        #region ContextMenuStrip Commands

        private void menuItemCopy_Click(object sender, EventArgs e)
        {
            PerformCopy();
        }

        private void menuItemProperties_Click(object sender, EventArgs e)
        {
            PerformViewHideProperties();
        }

        private void menuItemSave_Click(object sender, EventArgs e)
        {
            PerformSave();
        }

        #endregion // ContextMenuStrip Commands

        #region Misc

        #region dataGridView_MouseDown

        private void dataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            // To ensure that the user is going to save/copy the image under the cursor, 
            // make the row under the cursor the current item in the binding source.
            if (e.Button == MouseButtons.Right)
            {
                System.Windows.Forms.DataGridView.HitTestInfo info = dataGridView.HitTest(e.X, e.Y);
                if (info.RowIndex > -1)
                    bindingSource.Position = info.RowIndex;
            }
        }

        #endregion // dataGridView_MouseDown

        #region tabControl_SelectedIndexChanged

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            toolStripComboBox.Visible = tabControl.SelectedTab == tabPageIndividualImage;
            toolStripComboSeparator.Visible = tabControl.SelectedTab == tabPageIndividualImage;
        }
        #endregion // tabControl_SelectedIndexChanged

        #endregion // Misc

        #endregion // Event Handlers

        #region Private Helpers

        #region ExtractImagesFromAssembly

        private List<ImageInfo> ExtractImagesFromAssembly(Assembly assembly)
        {
            List<ImageInfo> imageInfos = new List<ImageInfo>();

            foreach (string name in assembly.GetManifestResourceNames())
            {
                using (Stream stream = assembly.GetManifestResourceStream(name))
                {
                    #region Icon

                    // Treat the resource as an icon.
                    try
                    {
                        Icon icon = new Icon(stream);
                        imageInfos.Add(new ImageInfo(icon, name));
                        continue;
                    }
                    catch (ArgumentException)
                    {
                        stream.Position = 0;
                    }

                    #endregion // Icon

                    #region Cursor

                    // Treat the resource as a cursor.
                    try
                    {
                        Cursor cursor = new Cursor(stream);
                        imageInfos.Add(new ImageInfo(cursor, name));
                        continue;
                    }
                    catch (ArgumentException)
                    {
                        stream.Position = 0;
                    }

                    #endregion // Cursor

                    #region Image

                    // Treat the resource as an image.
                    try
                    {
                        Image image = Image.FromStream(stream);

                        // If the image is an animated GIF, do not add it to the collection
                        // because the Image class cannot handle them and will throw an exception
                        // when the image is displayed.
                        FrameDimension frameDim = new FrameDimension(image.FrameDimensionsList[0]);
                        bool isAnimatedGif = image.GetFrameCount(frameDim) > 1;
                        if (!isAnimatedGif)
                            imageInfos.Add(new ImageInfo(image, name));
                        else
                            image.Dispose();

                        continue;
                    }
                    catch (ArgumentException)
                    {
                        stream.Position = 0;
                    }

                    #endregion // Image

                    #region Resource File

                    // Treat the resource as a resource file.
                    try
                    {
                        // The embedded resource in the stream is not an image, so
                        // read it into a ResourceReader and extract the values from there.
                        using (IResourceReader reader = new ResourceReader(stream))
                        {
                            foreach (DictionaryEntry entry in reader)
                            {
                                if (entry.Value is Icon)
                                {
                                    imageInfos.Add(new ImageInfo(entry.Value, name));
                                }
                                else if (entry.Value is Image)
                                {
                                    imageInfos.Add(new ImageInfo(entry.Value, name));
                                }
                                else if (entry.Value is ImageListStreamer)
                                {
                                    // Load an ImageList with the ImageListStreamer and
                                    // store a reference to every image it contains.
                                    using (ImageList imageList = new ImageList())
                                    {
                                        imageList.ImageStream = entry.Value as ImageListStreamer;
                                        for (int idx = 0; idx < imageList.Images.Count; idx++)
                                            imageInfos.Add(new ImageInfo(imageList.Images[idx], name + "_" + idx));
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }

                    #endregion // Resource File
                }
            }

            return imageInfos;
        }

        #endregion // ExtractImagesFromAssembly

        #region LoadAssembly

        private Assembly LoadAssembly(string assemblyPath, bool showErrorDialog)
        {
            try
            {
                // Use ReflectionOnlyLoadFrom to ensure that the assembly can be loaded.
                Assembly assembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);

                // Update the text/tooltip in the StatusStrip to the new assembly info.
                toolStripStatusLabel.Text = assembly.FullName;
                toolStripStatusLabel.ToolTipText = assembly.Location;

                return assembly;
            }
            catch (Exception)
            {
                if (showErrorDialog)
                {
                    MessageBox.Show(
                        "The specified file is not a .NET assembly.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                return null;
            }
        }

        #endregion // LoadAssembly

        #region LoadImagesFromAssembly

        private void LoadImagesFromAssembly(string assemblyPath)
        {
            // Try to load the assembly at the specified location.
            Assembly assembly = LoadAssembly(assemblyPath, true);
            if (assembly == null)
                return;

            currentAssembly = assembly;

            // Dispose of the images currently being displayed, if any.
            if (bindingSource.DataSource != null)
                foreach (ImageInfo imgInfo in bindingSource.DataSource as List<ImageInfo>)
                    imgInfo.Dispose();

            // Bind to a list of every image embedded in the assembly.
            bindingSource.DataSource = ExtractImagesFromAssembly(currentAssembly);
        }

        #endregion // LoadImagesFromAssembly

        #region PerformXXX

        private void PerformCopy()
        {
            if (pictureBox.Image != null)
                Clipboard.SetImage(pictureBox.Image);
        }

        private void PerformOpen()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Assemblies (*.exe, *.dll)|*.exe;*.dll";
                if (dlg.ShowDialog() == DialogResult.OK)
                    LoadImagesFromAssembly(dlg.FileName);
            }
        }

        private string GetExtension(ImageInfo imageInfo)
        {
            if (imageInfo.ResourceDetails.ImageType == ImageType.Icon)
                return ".ico";
            else if (imageInfo.ResourceDetails.ImageType == ImageType.Cursor)
                return " .cur";
            else
            {
                if (imageInfo.Image.RawFormat.Guid == ImageFormat.Jpeg.Guid)
                    return ".jpg";
                else if (imageInfo.Image.RawFormat.Guid == ImageFormat.Gif.Guid)
                    return ".gif";
                else if (imageInfo.Image.RawFormat.Guid == ImageFormat.Png.Guid)
                    return ".png";
                else
                    return ".bmp";
            }
        }

        private string GetFilter(string extension)
        {
            string name = "bmp";
            switch (extension)
            {
                case ".cur":
                    name = "Cursor";
                    break;
                case ".ico":
                    name = "Icon";
                    break;
                case ".png":
                    name = "Portable Network Graphic";
                    break;
                case ".jpg":
                    name = "JPEG";
                    break;
                default:
                    name = "Bitmap";
                    break;
            }

            return string.Format("{0} (*{1})|*{1}", name, extension);
        }

        private void PerformSave()
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                ImageInfo imageInfo = bindingSource.Current as ImageInfo;

                string extension = GetExtension(imageInfo);
                dlg.Filter = GetFilter(extension);
                dlg.FileName = imageInfo.ResourceDetails.ResourceName + extension;

                if (dlg.ShowDialog() == DialogResult.OK)
                    SaveImage(imageInfo, dlg.FileName);
            }
        }

        private void PerformSaveAll()
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select an output folder to save all images.";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string extension;
                    for (int i = 0; i < bindingSource.Count; i++)
                    {
                        ImageInfo imageInfo = bindingSource.Current as ImageInfo;
                        extension = GetExtension(imageInfo);

                        SaveImage(imageInfo, Path.Combine(dlg.SelectedPath, imageInfo.ResourceDetails.ResourceName + extension));

                        bindingSource.MoveNext();
                    }

                    MessageBox.Show(string.Format("{0} image(s) saved", bindingSource.Count), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                bindingSource.MoveFirst();
            }
        }

        private void PerformViewHideProperties()
        {
            splitContainer.Panel2Collapsed = !splitContainer.Panel2Collapsed;

            string text =
                splitContainer.Panel2Collapsed ?
                "View Properties" :
                "Hide Properties";

            propertiesToolStripButton.ToolTipText = text;
            menuItemProperties.Text = text;

            // If the tab control does not repaint now, a section of it will not be
            // drawn properly.
            tabControl.Refresh();
        }

        #endregion // PerformXXX

        #region SaveImage

        private void SaveImage(ImageInfo imageInfo, string fileName)
        {
            string extension = String.Empty;
            if (fileName.Length > 4)
                extension = fileName.Substring(fileName.Length - 4, 4);

            try
            {
                switch (extension)
                {
                    #region Icon

                    case ".ico":
                        // If, for some bizarre reason, someone tries to save a bitmap as an icon,
                        // ignore that request and just save it as a bitmap.
                        if (imageInfo.SourceObject is Icon == false)
                            goto default;

                        using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                            (imageInfo.SourceObject as Icon).Save(stream);
                        break;

                    #endregion // Icon

                    #region Cursor

                    case ".cur":
                        // If, for some bizarre reason, someone tries to save a bitmap as a cursor,
                        // ignore that request and just save it as a bitmap.
                        if (imageInfo.SourceObject is Cursor == false)
                            goto default;

                        // Copy the cursor byte-by-byte out of the assembly into a file.
                        using (Stream stream = currentAssembly.GetManifestResourceStream(imageInfo.ResourceDetails.ResourceName))
                        using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                        {
                            int i;
                            while ((i = stream.ReadByte()) > -1)
                                fileStream.WriteByte((byte)i);
                        }
                        break;

                    #endregion // Cursor

                    #region Bitmap
                    default:
                        // Copy the image to a new bitmap or else an error can occur while saving.
                        using (Bitmap bmp = new Bitmap(imageInfo.Image))
                            bmp.Save(fileName);
                        break;

                    #endregion // Bitmap
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An exception was thrown while trying to save the image: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion // SaveImage




        #endregion // Private Helpers
    }
}
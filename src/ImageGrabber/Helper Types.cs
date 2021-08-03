using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Diagnostics;

namespace ImageGrabber
{
    #region ImageInfo

    class ImageInfo : IDisposable
    {
        #region Data

        private Image image;
        private readonly ResourceDetails resourceDetails;

        private object sourceObject;

        #endregion // Data

        #region Constructor

        public ImageInfo(object sourceObject, string resourceName)
        {
            this.sourceObject = sourceObject;
            resourceDetails = new ResourceDetails(Image, DetermineImageType(), resourceName);
        }

        #endregion // Constructor

        #region Image

        public Image Image
        {
            get
            {
                if (image == null)
                {
                    if (sourceObject is Icon)
                    {
                        image = (sourceObject as Icon).ToBitmap();
                    }
                    else if (sourceObject is Cursor)
                    {
                        Cursor cursor = sourceObject as Cursor;
                        Size size = cursor.Size;
                        image = new Bitmap(size.Width, size.Height);
                        using (Graphics grfx = Graphics.FromImage(image))
                            cursor.Draw(grfx, new Rectangle(Point.Empty, size));
                    }
                    else if (sourceObject is Image)
                    {
                        image = sourceObject as Image;
                    }
                    else
                        Debug.Fail("Unexpected type of source object.");
                }

                return image;
            }
        }

        #endregion // Image

        #region DetermineImageType

        private ImageType DetermineImageType()
        {
            if (sourceObject is Icon)
                return ImageType.Icon;

            if (sourceObject is Cursor)
                return ImageType.Cursor;

            if (sourceObject is Image)
                return ImageType.Image;

            throw new ApplicationException("Unexpected type of source object.");
        }

        #endregion // DetermineImageType

        #region ResourceDetails

        public ResourceDetails ResourceDetails
        {
            get { return resourceDetails; }
        }

        #endregion // ResourceDetails

        #region SourceObject

        [Browsable(false)]
        public object SourceObject
        {
            get { return sourceObject; }
        }

        #endregion // SourceObject

        #region IDisposable Members

        public void Dispose()
        {
            if (image != null)
                image.Dispose();

            if (sourceObject is IDisposable)
                (sourceObject as IDisposable).Dispose();
        }

        #endregion // IDisposable Members
    }

    #endregion // ImageInfo

    #region ResourceDetails

    struct ResourceDetails
    {
        #region Data

        private readonly Image image;
        private readonly ImageType imageType;
        private readonly string resourceName;

        #endregion // Data

        #region Constructor

        public ResourceDetails(Image image, ImageType imageType, string resourceName)
        {
            this.image = image;
            this.imageType = imageType;
            this.resourceName = resourceName;
        }

        #endregion // Constructor

        #region Properties

        [Description("The horizontal resolution, in pixels per inch, of the image.")]
        [Category("Image Data")]
        public float HorizontalResolution
        {
            get { return image.HorizontalResolution; }
        }

        [Description("The type of file the image was stored as in the assembly.")]
        [Category("Resource Data")]
        public ImageType ImageType
        {
            get { return imageType; }
        }

        [Description("The width and height of the image.")]
        [Category("Image Data")]
        public SizeF PhysicalDimension
        {
            get { return image.PhysicalDimension; }
        }

        [Description("The pixel format of the image.")]
        [Category("Image Data")]
        public PixelFormat PixelFormat
        {
            get { return image.PixelFormat; }
        }

        [Description("The format of the image (an ImageFormat value).")]
        [Category("Image Data")]
        public ImageFormat RawFormat
        {
            get { return image.RawFormat; }
        }

        [Description("The name of the resource in the assembly.")]
        [Category("Resource Data")]
        public string ResourceName
        {
            get { return resourceName; }
        }

        [Description("The width and height, in pixels, of the image.")]
        [Category("Image Data")]
        public Size Size
        {
            get { return image.Size; }
        }

        [Description("The horizontal resolution, in pixels per inch, of the image.")]
        [Category("Image Data")]
        public float VerticalResolution
        {
            get { return image.VerticalResolution; }
        }

        #endregion // Properties
    }

    #endregion // ResourceDetails

    #region ImageType

    enum ImageType
    {
        Cursor,
        Icon,
        Image
    }

    #endregion // ImageType
}
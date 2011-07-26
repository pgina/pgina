using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace pGina.Shared.AuthenticationUI
{
    public class ImageElement : Element
    {
        protected Bitmap m_value = null;
        public Bitmap Bitmap
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public ImageElement(Bitmap value) :
            base(ElementType.TileImage, "Image")
        {
            Bitmap = value;
        }

        public ImageElement(string imageFileLocation) :
            base(ElementType.TileImage, "Image")
        {
            Bitmap = new Bitmap(imageFileLocation);
        }

        public override Guid UUid
        {
            get
            {
                return Constants.ImageElementUuid;
            }
            set
            {
                throw new ApplicationException("You cannot set a UUid on an image element, there can be only one");
            }
        }
    }
}

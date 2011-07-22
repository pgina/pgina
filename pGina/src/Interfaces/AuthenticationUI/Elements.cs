using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace pGina.Interfaces.AuthenticationUI
{
    public class Constants
    {
        public static readonly Guid ImageElementUuid = new Guid("{4BED6203-631D-424C-8005-CC335FC57D2D}");        
        public static readonly Guid UsernameElementUuid = new Guid("{53A104B5-3FAD-461A-A32D-47683A6A8C7E}");
        public static readonly Guid PasswordElementUuid = new Guid("{067828B4-4314-4498-944F-007F0BD2CA5B}");
    }

    public abstract class Element
    {
        public enum ElementType
        {
            Invalid,
            LargeText,
            SmallText,
            CommandLink,
            EditText,
            PasswordText,
            TileImage,
            Checkbox,
            Combobox,            
        }

        public enum ElementShowType
        {
            Hidden,
            ShowOnSelected,
            ShowOnDeselected,
            ShowInBoth,            
        }

        public enum ElementStyle
        {
            NothingSpecial,
            Readonly,
            Disabled,
            Focused            
        }

        protected string m_name = null;
        protected ElementType m_type = ElementType.Invalid;
        protected Guid m_guid = Guid.NewGuid();
        protected ElementShowType m_showtype = ElementShowType.ShowInBoth;
        protected ElementStyle m_style = ElementStyle.NothingSpecial;

        public virtual string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public virtual ElementType Type
        {
            get { return m_type; }
            set { m_type = value; }
        }

        public virtual Guid UUid
        {
            get { return m_guid; }
            set { m_guid = value; }
        }

        public virtual ElementShowType ShowType
        {
            get { return m_showtype; }
            set { m_showtype = value; }
        }

        public virtual ElementStyle Style
        {
            get { return m_style; }
            set { m_style = value; }
        }

        public Element()
        {
        }

        public Element(ElementType type, string name)            
        {
            m_type = type;
            m_name = name;
        }       
    }

    public abstract class TextElement : Element
    {
        protected string m_value = null;
        public string Text
        {
            get { return m_value; }
            set { m_value = value; }
        }

        protected TextElement(string name, ElementType type) :
            base(type, name) 
        { 
        }

        protected TextElement(string name, string value, ElementType type) :            
            this(name, type)
        {
            Text = value;
        }        
    }

    public class LargeTextElement : TextElement
    {
        public LargeTextElement(string name, string value) :
            base(name, value, ElementType.LargeText)
        {
        }
    }

    public class SmallTextElement : TextElement
    {
        public SmallTextElement(string name, string value) :
            base(name, value, ElementType.SmallText)
        {
        }
    }

    public class EditTextElement : TextElement
    {
        public EditTextElement(string name, string defaultValue) :
            base(name, defaultValue, ElementType.EditText)
        {
        }

        public EditTextElement(string name) :
            base(name, ElementType.EditText)
        {
        }

        protected EditTextElement(string name, Guid uuid) :
            this(name)            
        {
            UUid = uuid;
        }

        public static EditTextElement UsernameElement
        {
            get { return new EditTextElement("Username", Constants.UsernameElementUuid); }
        }
    }

    public class PasswordTextElement : TextElement
    {        
        public PasswordTextElement(string name, string defaultValue) :
            base(name, defaultValue, ElementType.PasswordText)
        {
        }
        
        public PasswordTextElement(string name) :
            base(name, ElementType.PasswordText)
        {
        }

        protected PasswordTextElement(string name, Guid uuid) :
            this(name)
        {
            UUid = uuid;
        }

        public static PasswordTextElement PasswordElement
        {
            get { return new PasswordTextElement("Password", Constants.PasswordElementUuid); }
        }
    }

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

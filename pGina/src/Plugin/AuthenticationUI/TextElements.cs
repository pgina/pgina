using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Shared.AuthenticationUI
{
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
}

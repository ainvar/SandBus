
namespace Ainvar.Bus.InProcess
{
    using System;
    using System.Collections.Generic;
    public class Dispatch : IDispatch
    {
        public Guid DispatchId { get; private set; }
        public Guid DispatchOwner { get; private set; }
        public Type DispatchOwnerType { get; private set; }
        public ContentType ContentType { get; private set; }

        public DateTime TimeStamp { get; private set; }

        public string Content { get; private set; }
        public string OwnerDescription { get; set; }
        public string OwnerGroup { get; set; }

        private IDictionary<string, string> _data;

        public Dispatch(string content, ContentType contentType, Guid dispatchOwner)
        : this(dispatchOwner, content)
        {
            ContentType = contentType;
        }

        public Dispatch(string content)
        {
            DispatchId = Guid.NewGuid();
            Content = content;
            TimeStamp = DateTime.Now;
            OwnerDescription = "none";
            ContentType = ContentType.TXT;
        }

        public Dispatch(Guid dispatchOwner, string content)
        : this(content)
        {
            DispatchOwner = dispatchOwner;
        }

        public Dispatch(Guid dispatchOwner, string ownerDescription, string content)
        : this(dispatchOwner, content)
        {
            OwnerDescription = ownerDescription;
        }

        public Dispatch(Guid dispatchOwner, string ownerDescription, Type dispatchOwnerType, string content)
        : this(dispatchOwner, ownerDescription, content)
        {
            DispatchOwnerType = dispatchOwnerType;
        }

        public virtual IDictionary<string, string> Data
        {
            get
            {
                if (this._data == null)
                {
                    _data = new Dictionary<string, string>();
                }
                return this._data;
            }
        }
    }
}

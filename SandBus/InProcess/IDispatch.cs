
namespace Ainvar.Bus.InProcess
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public interface IDispatch
    {
        Guid DispatchOwner { get; }
        Type DispatchOwnerType { get; }
        DateTime TimeStamp { get; }
        string Content { get; }
    }
}

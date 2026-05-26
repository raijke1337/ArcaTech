using Arcatech.Texts;

namespace Arcatech.UI
{
    public interface IIconContent
    {
        public Description Description { get; }

    }

    public interface IActionIconContent : IIconContent
    {
        public ActionIconDrawType IconDrawType { get; }
        public float FillValue { get; }
        public string StringInfo { get; }
    }

    public enum ActionIconDrawType
    {
        None,
        InternalCd,
        Clip,
        Charge,
        Queue
    }
    
}
namespace VRSprayPainting
{
    /// <summary>
    /// Anything that can receive paint at a UV coordinate.
    /// Keep this limited to applying paint (see architecture doc, Interface Segregation).
    /// </summary>
    public interface IPaintable
    {
        void ApplyPaint(in PaintStamp stamp);
    }
}

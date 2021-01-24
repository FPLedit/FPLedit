namespace FPLedit.Bildfahrplan.Render
{
    internal static class GdiAvailabilityTest
    {
        public static bool Test()
        {
            try
            {
                var bmp = new System.Drawing.Bitmap(1, 1);
                bmp?.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
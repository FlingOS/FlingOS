namespace Kernel.VGA.VMWare
{
    public static class SVGAII_Overlay
    {
        public enum OverlayFormat : uint
        {
            YV12 = 0x32315659,
            YUY2 = 0x32595559,
            UYVY = 0x59565955
        }

        public const uint VIDEO_COLOURKEY_MASK = 0x00FFFFFF;


        public static unsafe bool VideoGetAttributes(
            OverlayFormat format,
            uint* width, uint* height,
            uint* size, uint* pitches,
            uint* offsets)
        {
            uint tmp;

            *width = (*width + 1) & ~1u;

            if (offsets != null)
            {
                offsets[0] = 0;
            }

            switch (format)
            {
                case OverlayFormat.YV12:
                    *height = (*height + 1) & ~1u;
                    *size = (*width + 3) & ~3u;

                    if (pitches != null)
                    {
                        pitches[0] = *size;
                    }

                    *size *= *height;

                    if (offsets != null)
                    {
                        offsets[1] = *size;
                    }

                    tmp = ((*width >> 1) + 3u) & ~3u;

                    if (pitches != null)
                    {
                        pitches[1] = pitches[2] = tmp;
                    }

                    tmp *= (*height >> 1);
                    *size += tmp;

                    if (offsets != null)
                    {
                        offsets[2] = *size;
                    }

                    *size += tmp;
                    break;

                case OverlayFormat.YUY2:
                case OverlayFormat.UYVY:
                    *size = *width * 2;

                    if (pitches != null)
                    {
                        pitches[0] = *size;
                    }

                    *size *= *height;
                    break;

                default:
                    return false;
            }

            return true;
        }
    }
}

using Kernel.Framework;
using Kernel.Framework.Exceptions;
using Kernel.Utilities;

namespace Kernel.VGA.VMWare
{
    public static unsafe class Screen
    {
        public static void Init(SVGAII svga)
        {
            if (!(svga.HasFIFOCapability((uint)SVGAII_Registers.FIFO_Capabilities.SCREEN_OBJECT) ||
                  svga.HasFIFOCapability((uint)SVGAII_Registers.FIFO_Capabilities.SCREEN_OBJECT_2)))
            {
                BasicConsole.WriteLine("SVGA-II Screen : FIFO Screen Object 1 and 2 not supported.");
                ExceptionMethods.Throw(new ArgumentException("SVGA-II Screen FIFO screen object 1 and 2 not supported."));
            }
        }

        public static void Create(SVGAII svga, 
            SVGAII_Registers.ScreenObject* screen)
        {
            if (svga.HasFIFOCapability((uint)SVGAII_Registers.FIFO_Capabilities.SCREEN_OBJECT_2))
            {
                uint pitch = screen->Size.Width * sizeof(uint);
                uint size = screen->Size.Height * pitch;
                screen->StructSize = (uint)sizeof(SVGAII_Registers.ScreenObject);
                svga.AllocGMR(size, &screen->BackingStore.Ptr);
                screen->BackingStore.Ptr.Offset = 0;
                screen->BackingStore.Pitch = pitch;
            }
            else
            {
                screen->StructSize = (uint)((byte*)&screen->BackingStore - (byte*)screen);
            }

            Define(svga, screen);
        }

        public static void Define(SVGAII svga, 
            SVGAII_Registers.ScreenObject* screen)
        {
            SVGAII_Registers.FIFO_CommandDefineCursor* cmd =
                (SVGAII_Registers.FIFO_CommandDefineCursor*)svga.FIFOReserveCommand(
                    (uint)SVGAII_Registers.FIFO_Command.DEFINE_SCREEN, screen->StructSize);

            MemoryUtils.MemCpy((byte*)cmd, (byte*)screen, screen->StructSize);
            svga.FIFOCommitAll();
        }

        public static void Destroy(SVGAII svga, 
            uint id)
        {
            SVGAII_Registers.FIFO_CommandDestroyScreen* cmd = 
                (SVGAII_Registers.FIFO_CommandDestroyScreen*)svga.FIFOReserveCommand(
                (uint)SVGAII_Registers.FIFO_Command.DESTROY_SCREEN, 
                (uint)sizeof(SVGAII_Registers.FIFO_CommandDestroyScreen));

            cmd->ScreenId = id;
            svga.FIFOCommitAll();
        }

        public static void DefineGMRFB(SVGAII svga,
            SVGAII_Registers.GuestPointer ptr,
            uint BytesPerLine,
            SVGAII_Registers.GMRImageFormat format)
        {
            SVGAII_Registers.FIFO_CommandDefineGMRFB* cmd =
                (SVGAII_Registers.FIFO_CommandDefineGMRFB*)svga.FIFOReserveCommand(
                (uint)SVGAII_Registers.FIFO_Command.DEFINE_GMRFB,
                (uint)sizeof(SVGAII_Registers.FIFO_CommandDefineGMRFB));

            cmd->Pointer = ptr;
            cmd->BytesPerLine = BytesPerLine;
            cmd->Format = format;

            svga.FIFOCommitAll();
        }

        public static void BlitFromGMRFB(SVGAII svga,
            SVGAII_Registers.SignedPoint* srcOrigin,
            SVGAII_Registers.SignedRectangle* dstRect,
            uint dstScreen)
        {
            SVGAII_Registers.FIFO_CommandBlitGMRFBToScreen* cmd =
                (SVGAII_Registers.FIFO_CommandBlitGMRFBToScreen*)svga.FIFOReserveCommand(
                (uint)SVGAII_Registers.FIFO_Command.BLIT_GMRFB_TO_SCREEN,
                (uint)sizeof(SVGAII_Registers.FIFO_CommandBlitGMRFBToScreen));

            cmd->SrcOrigin = *srcOrigin;
            cmd->DstRectangle = *dstRect;
            cmd->DstScreenId = dstScreen;

            svga.FIFOCommitAll();
        }

        public static void BlitToGMRFB(SVGAII svga,
            SVGAII_Registers.SignedPoint* dstOrigin,
            SVGAII_Registers.SignedRectangle* srcRect,
            uint srcScreen)
        {
            SVGAII_Registers.FIFO_CommandBlitScreenToGMRFB* cmd =
                (SVGAII_Registers.FIFO_CommandBlitScreenToGMRFB*)svga.FIFOReserveCommand(
                (uint)SVGAII_Registers.FIFO_Command.BLIT_SCREEN_TO_GMRFB,
                (uint)sizeof(SVGAII_Registers.FIFO_CommandBlitScreenToGMRFB));

            cmd->DstOrigin = *dstOrigin;
            cmd->SrcRectangle = *srcRect;
            cmd->SrcScreenId = srcScreen;

            svga.FIFOCommitAll();
        }

        public static void AnnotateFill(SVGAII svga,
            SVGAII_Registers.ColourBGRX colour)
        {
            SVGAII_Registers.FIFO_CommandAnnotationFill* cmd =
                (SVGAII_Registers.FIFO_CommandAnnotationFill*)svga.FIFOReserveCommand(
                (uint)SVGAII_Registers.FIFO_Command.ANNOTATION_FILL,
                (uint)sizeof(SVGAII_Registers.FIFO_CommandAnnotationFill));

            cmd->Colour = colour;

            svga.FIFOCommitAll();
        }

        public static void AnnotateCopy(SVGAII svga,
            SVGAII_Registers.SignedPoint* srcOrigin,
            uint srcScreen)
        {
            SVGAII_Registers.FIFO_CommandAnnotationCopy* cmd =
                (SVGAII_Registers.FIFO_CommandAnnotationCopy*)svga.FIFOReserveCommand(
                (uint)SVGAII_Registers.FIFO_Command.ANNOTATION_COPY,
                (uint)sizeof(SVGAII_Registers.FIFO_CommandAnnotationCopy));

            cmd->SrcOrigin = *srcOrigin;
            cmd->SrcScreenId = srcScreen;

            svga.FIFOCommitAll();
        }
    }
}

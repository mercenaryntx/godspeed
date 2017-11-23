using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Stfs.Data
{
    public class CachedUserFlags : BinaryModelBase
    {
        [BinaryData]
        public virtual byte PaymentInstrumentCreditCard { get; set; }

        [BinaryData(1)]
        public virtual Country Country { get; set; }

        [BinaryData(1)]
        protected virtual int ThirdByte { get; set; }

        [BinaryData(1)]
        protected virtual int ForthByte { get; set; }

        public SubscriptionTeir SubscriptionTeir //Bits 16-19
        {
            get { return (SubscriptionTeir) (ThirdByte & ~0xFFFFFFF0); }
            set { ThirdByte = (ThirdByte & ~0x0F) | (int)value; }
        }

        public bool ParentalControlsEnabled //Bit 24
        {
            get { return (ForthByte & ~0xFFFFFFFE) == 1; }
            set { ForthByte = (ForthByte & ~0x01) | (value ? 1 : 0); }
        }

        public int Language //Bits 25-29
        {
            get { return (int)(ForthByte & ~0xFFFFFF1E); }
            set { ForthByte = (ForthByte & ~0xE1) | value; }
        }

        public CachedUserFlags(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}
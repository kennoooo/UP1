namespace WpfApp1
{
    public partial class Bookings
    {
        public decimal Total
        {
            get { return Price * Num; }
        }
    }
}

namespace UGV.XBee
{
    // feel free to modify this as we set up the structure of the packets we
    // are sending as JSON through the xbee
    public struct XBeePacket
    {
        public int latitude, longitude;
        public XBeePacket(int lat, int lng)
        {
            latitude = lat;
            longitude = lng;
        }

        public override bool Equals(object obj)
        {
            XBeePacket xbee = (XBeePacket)obj;

            return latitude == xbee.latitude &&
                longitude == xbee.longitude;
        }

        public override int GetHashCode()
        {
            return (latitude + 90) * 180 + longitude;
        }

        public static bool operator ==(XBeePacket left, XBeePacket right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(XBeePacket left, XBeePacket right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return "latitude: " + latitude + "\nlongitude: " + longitude;
        }
    }
}

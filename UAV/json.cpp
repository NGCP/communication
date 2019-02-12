#include "ThorSerialize/Traits.h"
#include "ThorSerialize/JsonThor.h"

struct XbeePacket {
	int latitude;
	int longitude;
};
ThorsAnvil_MakeTrait(XbeePacket, latitude, longitude);

int main() {
	XbeePacket packet{1,2};
	std::stringstream s;
	s << ThorsAnvil::Serialize::jsonExport(packet, ThorsAnvil::Serialize::PrinterInterface::OutputType::Stream);
}
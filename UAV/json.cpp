#include <msgpack.hpp>
#include <iostream>

using namespace std;

class ConnectMessage {
private:
	string type;
	unsigned long int id;
	unsigned long int sid;
	unsigned long int tid;
	unsigned long long int time;
	
public:
	ConnectMessage(float i, unsigned long int s, unsigned long int t, unsigned long long int ti) {
		type = "connect";
		id = i;
		sid = s;
		tid = t;
		time = ti;
	}
	
	ConnectMessage() {
		type = "connect";
		id = 0;
		sid = 0;
		tid = 0;
		time = 0;
	}
	
	string toString() {
		return "type: " + type + " id: " + to_string(id) + " sid: " + to_string(sid) + " tid: " + to_string(tid) + " time: " + to_string(time);
	}

	MSGPACK_DEFINE(type, id, sid, tid, time);
};

int main() {
	ConnectMessage packet(0, 100, 0, (std::chrono::system_clock::now().time_since_epoch()).count());
	
	cout << packet.toString() << endl;
	
	msgpack::sbuffer sbuf;
	msgpack::pack(sbuf, packet);
	
	msgpack::object_handle oh = msgpack::unpack(sbuf.data(), sbuf.size());
	msgpack::object obj = oh.get();
	
	ConnectMessage packet2;
	obj.convert(packet2);
	
	cout << packet2.toString() << endl;
}
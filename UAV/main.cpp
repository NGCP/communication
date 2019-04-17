#include <iostream>
#include <iomanip>
#include "json.hpp"

using nlohmann::json;

namespace message {
  struct message {
  	std::string type;
  	uint32_t id;
  	uint32_t sid;
  	uint32_t tid;
  	uint64_t time;
  };

  void to_json(json& j, const message& msg) {
    j = json{
      {"type", msg.type},
      {"id", msg.id},
      {"sid", msg.sid},
      {"tid", msg.tid},
      {"time", msg.time}
    };
  }

  void from_json(const json& j, message& msg) {
    j.at("type").get_to(msg.type);
    j.at("id").get_to(msg.id);
    j.at("sid").get_to(msg.sid);
    j.at("tid").get_to(msg.tid);
    j.at("time").get_to(msg.time);
  }

  struct connect : message {
    std::vector<std::string> jobsAvailable;
  };

  void to_json(json& j, const connect& msg) {
    to_json(j, (message) msg);
    j.push_back({"jobsAvailable", msg.jobsAvailable});
  }
  
  void from_json(const json& j, connect& msg) {
  	from_json(j, msg);
  	j.at("jobsAvailable").get_to(msg.jobsAvailable);
  }
  
  struct connectionAck : message {};
  
  void to_json(json& j, const connectionAck& msg) {
  	to_json(j, (message) msg);
  }
  
  void from_c_json(const json& j, connectionAck& msg) {
  	from_json(j, msg);
  }
}

uint64_t time() {
	return (std::chrono::system_clock::now().time_since_epoch()).count();
}

int main() {
	// INTERACTION WITH BASE MESSAGE STRUCT
 	
	// 1. Create message
  message::message msg {"test", 0, 0, 0, time()};
  
  // 2. Create JSON
  json msg_json;
  to_json(msg_json, msg);
  std::cout << "MESSAGE JSON" << std::endl;
  std::cout << msg_json.dump() << std::endl;
  
  // 3. Create msgpack (send the msg_msgpack variable through Xbee)
  std::vector<uint8_t> msg_msgpack = json::to_msgpack(msg_json);
  std::cout << "MESSAGE MSGPACK" << std::endl;
  for (auto& byte : msg_msgpack) {
  	std::cout << std::hex << std::setw(2) << std::setfill('0') << int(byte) << " ";
  }
  std::cout << std::endl;
  
  // INTERACTION WITH BUILDING A CONNECT MESSAGE STRUCT
  
  // 1. Create connect message
  message::connect connect_msg {"connect", 0, 100, 0, time(), {"isrSearch", "payloadDrop"}};
  
  // 2. Create JSON
  json connect_json;
  message::to_json(connect_json, connect_msg);
  std::cout << "\nCONNECT MESSAGE JSON" << std::endl;
  std::cout << connect_json.dump() << std::endl;
  
  // 3. Create msgpack (send the connect_msgpack variable through Xbee)
  std::vector<uint8_t> connect_msgpack = json::to_msgpack(connect_json);
  std::cout << "MESSAGE MSGPACK" << std::endl;
  for (auto& byte : connect_msgpack) {
  	std::cout << std::hex << std::setw(2) << std::setfill('0') << int(byte) << " ";
  }
  std::cout << std::endl;
  connect_msgpack.clear();
  
  // INTERACTION WITH TAKING A CONNECTIONACK MESSAGE STRUCT
	
	// 1. Get the msgpack (this comes from the Xbee)
	std::vector<uint8_t> connectionack_msgpack {0x85, 0xa4, 0x74, 0x79, 0x70, 0x65, 0xad, 0x63, 0x6f, 0x6e, 0x6e, 0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x41, 0x63, 0x6b, 0xa2, 0x69, 0x64, 0x00, 0xa3, 0x73, 0x69, 0x64, 0x00, 0xa3, 0x74, 0x69, 0x64, 0x64, 0xa4, 0x74, 0x69, 0x6d, 0x65, 0xcd, 0x27, 0x10};
	json connectionack_json = json::from_msgpack(connectionack_msgpack);
	std::cout << "\nCONNECTIONACK MESSAGE JSON" << std::endl;
	std::cout << connectionack_json.dump(4) << std::endl; // Putting 4 in argument will make JSON print pretty, and have 4 indentation.
	
	// 2. Get connectionAck message
	message::connectionAck connectionack_msg {"", 0, 0, 0, 0};
	message::from_c_json(connectionack_json, connectionack_msg);
	std::cout << "CONNECTIONACK STRUCT" << std::endl;
	std::cout << "type: " << connectionack_msg.type << std::endl;
	std::cout << "id: " << connectionack_msg.id << std::endl;
	std::cout << "sid: " << connectionack_msg.sid << std::endl;
	std::cout << "tid: " << connectionack_msg.tid << std::endl;
	std::cout << "time: " << connectionack_msg.time << std::endl;
}

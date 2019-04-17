#include <iostream>
#include <vector>
#include "json.hpp"


using nlohmann::json;

namespace message {
  struct message {
  	std::string type;
  	unsigned long int id;
  	unsigned long int sid;
  	unsigned long int tid;
  	long long int time;
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

  void from_json(const json& j, const message& msg) {
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

  void from_json(const json& j, connect &msg) {
    from_json(j, (message) msg);
	j.at("jobsAvailable").get_to(msg.jobsAvailable);
  }
}

int main() {
  return 0;
}

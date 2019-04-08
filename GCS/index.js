const SerialPort = require('serialport');
const { constants: C, XBeeAPI } = require('xbee-api');

const PORT = 'COM5';
const DESTINATION_MAC = '0013A20040917A31';

const port = new SerialPort(PORT, { baudRate: 57600 }, err => {
	if (err) {
		console.log(err.message);
		console.log('Failed to initialize XBee');
		return;
	}
});

const xbeeAPI = new XBeeAPI();

const data = {
  latitude: 69,
  longitude: 10,
}

port.pipe(xbeeAPI.parser);
xbeeAPI.builder.pipe(port);

port.on('open', () => {
  setInterval(() => {
	console.log('<< Sent');
	console.log(data);
	console.log();
    xbeeAPI.builder.write({
      type: C.FRAME_TYPE.ZIGBEE_TRANSMIT_REQUEST,
      destination64: DESTINATION_MAC,
      data: JSON.stringify(data),
    });
  }, 5000);
});

xbeeAPI.parser.on('data', frame => {
  if (frame.type !== C.FRAME_TYPE.ZIGBEE_RECEIVE_PACKET) return;

  const json = frame.data.toString();
  console.log('>> Received');
  console.log(JSON.parse(json));
  console.log();
});

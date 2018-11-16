const SerialPort = require('serialport');
const { costants: C, XBeeAPI } = require('xbee-api');

const SELF_XBEE_PORT = 'COM3';
const DESTINATION_XBEE_PORT = '0013A20040917A31';

const port = new SerialPort(SELF_XBEE_PORT, { baudRate: 57600 });
const xbeeAPI = new XBeeAPI({ api_mode: 1 });

let id;

serialport.pipe(xbeeAPI.parser);
xbeeAPI.builder.pipe(serialport);

serialport.on('open', () => {
  setInterval(() => {
    id = xbeeAPI.nextFrameId();

    xbeeAPI.builder.write({
      id: id,
      type: C.FRAME_TYPE.ZIGBEE_TRANSMIT_REQUEST,
      destination64: DESTINATION_XBEE_PORT,
      data: 'Hello world',
    });
  }, 1000);
});

xbeeAPI.parser.on('data', console.log);

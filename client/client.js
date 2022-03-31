const { Client } = require('m2m');

let client = new Client();

client.connect(() => {

  // test payload
  //let payload = {type:'random'};

  //client.sendData({id:500, channel:'ipc-channel', payload:payload}, (data) => {
  client.watch({id:500, channel:'ipc-channel'}, (data) => {  
    try{
      let jd = JSON.parse(data);
      console.log('rcvd json data:', jd);
    }
    catch (e){
      console.log('rcvd string data:', data);
    }
  });

  //client.sendData({id:600, channel:'ipc-channel', payload:payload}, (data) => {
  client.watch({id:600, channel:'ipc-channel'}, (data) => {  
    try{
      let jd = JSON.parse(data);
      console.log('rcvd json data:', jd);
    }
    catch (e){
      console.log('rcvd string data:', data);
    }
  });

});

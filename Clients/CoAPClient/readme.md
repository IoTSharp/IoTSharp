Example:

Telemetry:
CoAPClient.exe  POST coap://<_server_>/Telemetry?<_token_>   <_playload_>

CoAPClient.exe  POST coap://localhost/Telemetry?3cb97cd31fbc40b08d12ec47a6fad622   {\"aaa\":\"bbb\"}

Attributes:

CoAPClient.exe  POST coap://<_server_>/Attributes?<_token_>   <_playload_>

CoAPClient.exe  POST coap://localhost/Attributes?3cb97cd31fbc40b08d12ec47a6fad622   {\"aaa\":\"bbb\"}


URI format:

Json:
POST coap://localhost/Attributes?3cb97cd31fbc40b08d12ec47a6fad622 

Xml document：
POST coap://localhost/Attributes?3cb97cd31fbc40b08d12ec47a6fad622&xml&keyname       

binary:
POST coap://localhost/Attributes?3cb97cd31fbc40b08d12ec47a6fad622&binary&keyname    
<?php
namespace iboxs\websocket\event\msgEvent;

use iboxs\websocket\common\Encrypt;

class Connected extends Base {
    public function aeskey(){
        $key=$this->msgData['aesKey'];
        $me=$this->msgData['me'];
        $privateKey=file_get_contents(config('websocket.chatrsa.private_key'));
        $aesKey=Encrypt::rsaDecrypt($key,$privateKey);
        redis('iboxsWebsocket:chat:aesKey:'. $this->clientID,$aesKey,3*3600);
        $userID=$this->onEvent('connectedUser',$me,$aesKey);
        if($userID){
            $this->userID=$userID;
            redis('iboxsWebsocket:chat:userID:'. $this->clientID,$userID,3*3600);
            redis('iboxsWebsocket:chat:clientID:'. $this->userID,$this->clientID,3*3600);
        }
        $this->replyMsg('connected','confirm',['user_id'=>$userID]);
    }
}

?>
<?php
namespace iboxs\websocket\event\msgEvent;

use iboxs\websocket\common\Encrypt;

class Heart extends Base {
    public function heart(){
        $this->onEvent('heart',$this->userID,$this->clientID);
    }
}
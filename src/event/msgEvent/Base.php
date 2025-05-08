<?php
namespace iboxs\websocket\event\msgEvent;
use iboxs\websocket\common\Socket;

class Base{
    protected $replyID='';
    protected $header=[];
    protected $encrypt=false;
    protected $msgID;
    protected $msgData;
    protected $clientID;
    protected $userID=0;

    public function __construct($data,$clientID)
    {
        $this->replyID=$data['reply_id'];
        $this->header=$data['header'];
        $this->encrypt=$data['encrypt']??true;
        $this->msgID=$data['msg_id'];
        $this->msgData=$data['data'];
        $this->clientID=$clientID;
        $this->userID=redis('iboxsWebsocket:chat:userID:'. $this->clientID);
    }

    public function onEvent($method,...$args){
        $class=config('websocket.msgEvent.event');
        if($class==false){
            return;
        }
        if(!class_exists($class)){
            throw new \Exception('消息反馈类['.$class.']不存在');
        }
        $obj=new $class();
        return $obj->$method(...$args);
    }

    public function replyMsg($type,$subType,$data=[]){
        Socket::sendMsgClient($this->clientID,$type,$subType,$data,true,0,$this->userID);
    }
}
?>

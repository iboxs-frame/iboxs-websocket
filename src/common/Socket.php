<?php
namespace iboxs\websocket\common;

use common\Queue;
use GatewayWorker\Lib\Gateway;
use model\logmodel\SocketMsgLog;

class Socket{

    public static function sendMsgClient($clientID,$type,$subType,$data,$encrypt=true,$sendUserID=0,$userID=0){
        $msgID=Snowflake::createNo();
        $now=time();
        $header=[];
        $sign=md5($msgID.$type.$subType."IBOXS".$now);
        $msg=[
            'type'=>$type,
            'sub_type'=>$subType,
            'reply_id'=>0,
            'header'=>$header,
            'data'=>$data,
            'now'=>$now,
            'msg_id'=>$msgID,
            'sign'=>$sign,
            'encrypt'=>$encrypt
        ];
        self::send($clientID,$msg,$msgID,$encrypt,$sendUserID,$userID);
    }

    private static function encrypt($msg,$clientID){
        $data=$msg['data'];
        if(!is_string($data)){
            $data=json_encode($data,JSON_UNESCAPED_UNICODE);
        }
        $aesKey=redis('iboxsWebsocket:chat:aesKey:'. $clientID);
        if($aesKey==null){
            throw new \Exception('消息加密秘钥['.$clientID.']不存在，也可能已过期');
        }
        $iv=config('websocket.chatAesIv');
        dump('消息加密',$data,$aesKey,$iv);
        $data=Encrypt::aesEncrypt($data,$aesKey,$iv);
        $msg['data']=$data;
        return $msg;
    }

    private static function send($clientID,$msg,$msgID,$encrypt=false,$sendUserID=0,$userID=0){
        $log=[
            'send_user'=>$sendUserID,
            'client_id'=>$clientID,
            'reply_id'=>$msg['reply_id'],
            'msg'=>$msg,
            'send_time'=>microtime(true)
        ];
        if($encrypt){
            $sendmsg=self::encrypt($msg,$clientID);
        } else{
            $sendmsg=$msg;
        }
        echo '发送消息['.$clientID.']:'. json_encode($sendmsg)."\n";
        $send=Gateway::sendToClient($clientID,json_encode($sendmsg));
        $failModel=config('websocket.failmodel');
        if($failModel){
            $failModel::where('msg_id',$msgID)->delete();
        }
        if($send){
            Queue::pushLowMsg('insertdb',[
                'model'=>SocketMsgLog::class,
                'data'=>$log
            ]);
        } else{
            if($failModel){
                $failModel::insert([
                    'send_user'=>$sendUserID,
                    'receive_user'=>$userID,
                    'data'=>json_encode($msg['data']),
                    'msg_id'=>$msgID,
                    'send_time'=>time(),
                    'type'=>$msg['type'],
                    'sub_type'=>$msg['sub_type'],
                ]);
            }
        }
        return;
    }
}

?>
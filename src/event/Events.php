<?php
namespace iboxs\websocket\event;

use iboxs\websocket\common\Encrypt;
use iboxs\websocket\common\Socket;
use iboxs\websocket\event\msgEvent\Connected;

class Events {
    /**
    * 当客户端发来消息时触发
    * @param int $client_id 连接id
    * @param mixed $data 具体消息
    */
    public static function onMessage($client_id, $data){
        echo date('Y-m-d H:i:s')."收到消息【{$client_id}】：{$data}\n";
        /**
         * 标准接收消息体：
        */
        // $msg=[
        //     'type'=>'text',
        //     'sub_type'=>'text',
        //     'reply_id'=>'',  //如果是回复消息，这里是回复的消息ID
        //     'header'=>[],   //数据头（一些需要携带的明文数据放在这）
        //     'data'=>$data,  //加密的（AES加密，秘钥和向量在前期进行发送确认）
        //     'msg_id'=>'',
        //     'now'=>'',  //当前时间戳
        //     'sign'=>'' //md5(加密后的数据前100位+时间戳+md5(签名秘钥+通信秘钥))
        // ];
        $data=json_decode($data,true);
        if($data==null){
            return;
        }
        $checkSign=self::checkSign($data,$client_id);
        if(!$checkSign){
            $class=config('websocket.msgEvent.noSign');
            if($class==false){
                return;
            } else if(!class_exists($class)){
                throw new \Exception('消息处理类[noSign]不存在');
            }
            $obj=new $class();
            $obj->checkSignFail($client_id,$data);
            return;
        }
        $type=$data['type'];
        $subType=$data['sub_type'];
        $encrypt=$data['encrypt'];
        if($encrypt){
            $jsonData=self::decrypt($data['data'],$client_id);
            if($jsonData==false){
                return;
            }
            $data['data']=$jsonData;
            dump('消息解密完成',$data);
        }
        $class="iboxs\\websocket\\event\\msgEvent\\".ucfirst($type);
        dump('消息处理类',$class);
        if(!class_exists($class)){
            $class=config('websocket.msgEvent.noClass');
            dump('[空]消息处理类',$class);
            if($class==false){
                return;
            } else if(!class_exists($class)){
                echo '消息处理类不存在';
                throw new \Exception('消息处理类不存在');
            }
            $obj=new $class();
            $obj->noHandleClass($client_id,$data);
            return;
        }
        $obj=new $class($data,$client_id);
        if(!method_exists($obj,$subType)){
            echo '消息处理函数['.$subType.']不存在';
            throw new \Exception('消息处理函数['.$subType.']不存在');
        }
        $obj->$subType();
    }

    private static function decrypt($data,$client_id){
        $aesKey=redis('iboxsWebsocket:chat:aesKey:'. $client_id);
        if($aesKey==null){
            return false;
        }
        $iv=config('websocket.chatAesIv');
        $json=Encrypt::aesDecrypt($data,$aesKey,$iv);
        return json_decode($json,true);
    }

    private static function checkSign($data,$client_id){
        $encrypt=$data['encrypt']??true;
        if($encrypt){
            $msgData=$data['data'];
            $d='';
            if(strlen($msgData)>100){
                $d=substr($msgData,0,100);
            } else{
                $d=$msgData;
            }
            $aesKey=redis('iboxsWebsocket:chat:aesKey:'. $client_id);
            if($aesKey==null){
                return false;
            }
            $tmpSign=md5($d.$data['now'].md5($aesKey.config('websocket.chatkey')));
        } else{
            $tmpSign=md5($data['now'].md5(config('websocket.chatkey')));
        }
        return $tmpSign==$data['sign'];
    }

    /**
     * 当客户端连接时触发
     * 如果业务不需此回调可以删除onConnect
     *
     * @param int $client_id 连接id
     */
    public static function onConnect($client_id)
    {
        echo date('Y-m-d H:i:s')."客户端链接【{$client_id}】\n";
        // echo config('websocket.chatrsa.public_key');
        Socket::sendMsgClient($client_id,'connected','publickey',[
            'publickey'=>file_get_contents(config('websocket.chatrsa.public_key'))
        ],false);
    }
    
    /**
     * 当连接断开时触发的回调函数
     * @param $connection
     */
    public static function onClose($client_id){
        echo date('Y-m-d H:i:s')."客户端断开【{$client_id}】\n";
    }
    /**
     * 当客户端的连接上发生错误时触发
     * @param $connection
     * @param $code
     * @param $msg
     */
    public static function onError($client_id, $code, $msg)
    {
        echo date('Y-m-d H:i:s')."客户端错误【{$client_id}】CODE:{$code}:{$msg}\n";
    }
    /**
     * 每个进程启动
     * @param $worker
     */
    public static function onWorkerStart($worker)
    {

    }

    // 平滑重启
    public static function onWorkerReload($worker){

    }
}

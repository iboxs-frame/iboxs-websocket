<?php 

use Workerman\Worker;
use GatewayWorker\Register;

// 自动加载类
require_once vendor_path() . '/autoload.php';

// register 必须是text协议
$register = new Register('text://0.0.0.0:'. config('websocket.serverPort','1230'));

// 如果不是在根目录启动，则运行runAll方法
if(!defined('GLOBAL_START'))
{
    Worker::runAll();
}
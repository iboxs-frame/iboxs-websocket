<?php
namespace iboxs\websocket\common;

use iboxs\redis\Redis;

class Snowflake
{
    private $epoch = 1730390400000; // 自定义起始时间戳
    private $workerId;               // 工作节点ID，需要保证唯一性
    private $dataCenterId = 0;       // 数据中心ID
    private $sequence = 0;           // 初始序列号
    private $sequenceMask = 4095;    // 序列号掩码（12位）
    private $workerIdShift = 12;     // 工作节点ID左位移量(3位)
    private $dataCenterIdShift = 56; // 数据中心ID左位移量（3位）
    private $timestampLeftShift = 15;// 时间戳左位移量（40位/17位）
 
    public function __construct($epoch=1730390400000)
    {
        $this->epoch = $epoch;
        $this->dataCenterId = env('SNOWFLAKE.DATACENTERID',0);
        if($this->dataCenterId < 0 || $this->dataCenterId > 8){
            throw new \Exception('Worker ID must be between 0 and 7');
        }
        $workerId = env('SNOWFLAKE.WORKERID',1);
        if ($workerId > 0 && $workerId < 8) {
            $this->workerId = $workerId;
        } else {
            throw new \Exception('Worker ID must be between 0 and 7');
        }
    }

    public function createID()
    {
        $currentMillis = round(microtime(true) * 1000); // 获取当前毫秒时间戳
        $this->sequence=Redis::basic()->inc('test:'.$currentMillis);
        $this->sequence--;
        if($this->sequence>$this->sequenceMask){
            $this->sequence=0;
        }
        Redis::basic()->expire('test:'.$currentMillis,2);
        if ($currentMillis < $this->epoch) {
            throw new \Exception("Clock moved backwards. Refusing to generate id");
        }
        return (
            ($this->dataCenterId << $this->dataCenterIdShift) |
            (($currentMillis - $this->epoch) << $this->timestampLeftShift) |
            ($this->workerId << $this->workerIdShift) |
            $this->sequence
        );
    }

    public static function createNo($epoch=1730390400000){
        return (new self($epoch))->createID();
    }
}
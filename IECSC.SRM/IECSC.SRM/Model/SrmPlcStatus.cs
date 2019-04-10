using System.ComponentModel;

namespace IECSC.SRM
{
    public class SrmPlcStatus
    {
        /// <summary>
        /// PLC传递名称
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 心跳信息
        /// </summary>
        public int HeartBeat { get; set; }
        /// <summary>
        /// 设备状态
        /// </summary>
        public int OperateMode { get; set; }
        /// <summary>
        /// 任务状态
        /// </summary>
        public int MissionState { get; set; }
        /// <summary>
        /// 任务类型
        /// </summary>
        public int MissionType { get; set; }
        /// <summary>
        /// 任务编号
        /// </summary>
        public int MissionId { get; set; }
        /// <summary>
        /// RFID代码
        /// </summary>
        public string PalletNo { get; set; }
        /// <summary>
        /// 前叉当前列
        /// </summary>
        public int ActPosBay { get; set; }
        /// <summary>
        /// 当前层
        /// </summary>
        public int ActPosLevel { get; set; }
        /// <summary>
        /// 当前水平位置
        /// </summary>
        public int ActPosX { get; set; }
        /// <summary>
        /// 当前垂直位置
        /// </summary>
        public int ActPosY { get; set; }
        /// <summary>
        /// 浅库位货叉当前位置
        /// </summary>
        public int ActPosZ { get; set; }
        /// <summary>
        /// 叉深库位货叉当前位置
        /// </summary>
        public int ActPosZDeep { get; set; }
        /// <summary>
        /// 当前行走速度
        /// </summary>
        public int ActSpeedX { get; set; }
        /// <summary>
        /// 当前升降速度
        /// </summary>
        public int ActSpeedY { get; set; }
        /// <summary>
        /// 浅库位叉货当前速度
        /// </summary>
        public int ActSpeedZ { get; set; }
        /// <summary>
        /// 叉深库位货叉当前速度
        /// </summary>
        public int ActSpeedZDeep { get; set; }
        /// <summary>
        /// 负载状态
        /// </summary>
        public int LoadStatus { get; set; }
        /// <summary>
        /// 故障代码
        /// </summary>
        public int FaultNo { get; set; }
        /// <summary>
        /// 备用
        /// </summary>
        public int NoFunction { get; set; }
    }
}

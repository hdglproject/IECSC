
namespace IECSC.SRM
{
    public class SrmOpcItem
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipNo { get; set; }
        /// <summary>
        /// 测点长名
        /// </summary>
        public string TagLongName { get; set; }
        /// <summary>
        /// 测点名称
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// 测点索引
        /// </summary>
        public int TagIndex { get; set; }
        /// <summary>
        /// 业务唯一标示
        /// </summary>
        public string BusIdentity { get; set; }
    }
}

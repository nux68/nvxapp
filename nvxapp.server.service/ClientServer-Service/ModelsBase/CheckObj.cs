namespace nvxapp.server.service.ClientServer_Service.ModelsBase
{
    public class CheckObjOn_Id_Text : ICheckObj<string>
    {
        public string Id { get; set; } = string.Empty;
        public bool Checked { get; set; }
    }

    public class CheckObjOn_Id_Number : ICheckObj<int>
    {
        public int Id { get; set; } = 0;
        public bool Checked { get; set; }
    }


    public interface ICheckObj<T>
    {
        T Id { get; set; }
        bool Checked { get; set; }
    }
}

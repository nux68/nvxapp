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


    public class CheckObjOn_Id<T> : ICheckObj<T>
    {
        public T Id { get; set; }
        public bool Checked { get; set; }

        public CheckObjOn_Id(T id, bool checkedValue)
        {
            Id = id;
            Checked = checkedValue;
        }
    }

    public interface ICheckObj<T>
    {
        T Id { get; set; }
        bool Checked { get; set; }
    }
}

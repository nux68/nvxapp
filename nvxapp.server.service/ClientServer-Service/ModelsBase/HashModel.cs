using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.ModelsBase
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HashFieldAttribute : Attribute
    {
    }

    public class HashModel
    {
        public string Hash { get; set; } = string.Empty;

        public string CalcolaHash<T>(T instance, params Expression<Func<T, object>>[] campi)
        {
            var values = new List<string>();

            foreach (var campo in campi)
            {
                var compiled = campo.Compile();
                var value = compiled(instance);

                values.Add(value?.ToString() ?? "null");
            }

            var raw = string.Join("|", values);

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hashBytes = sha.ComputeHash(bytes);

            return Convert.ToHexString(hashBytes);
        }

        public string CalcolaHashOnAttribute()
        {
            var props = this.GetType()
                            .GetProperties()
                            .Where(p => Attribute.IsDefined(p, typeof(HashFieldAttribute)));

            var values = props
                .Select(p => p.GetValue(this)?.ToString() ?? "null")
                .ToList();

            var raw = string.Join("|", values);

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hashBytes = sha.ComputeHash(bytes);

            return Convert.ToHexString(hashBytes);
        }

        public bool IsHashChanged(string oldHash)
        {
            var newHash = CalcolaHashOnAttribute();
            return !string.Equals(oldHash, newHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}

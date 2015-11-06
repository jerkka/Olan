using System.Threading.Tasks;

namespace Olan {
    public delegate TTask AsyncEvent<TSender, TTask>(TSender sender) where TTask : Task;
}
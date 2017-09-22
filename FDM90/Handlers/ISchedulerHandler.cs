using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Handlers
{
    public interface ISchedulerHandler
    {
        string PostNow(ScheduledPost newPost);
        string CreateScheduledPost(ScheduledPost newPost);
        string UpdateScheduledPost(ScheduledPost updatedPost);
        IEnumerable<ScheduledPost> GetSchedulerPostsForUser(Guid userId);
        void SchedulerPostsForTime(DateTime currentTime);
        void DeleteScheduledPost(Guid postId);
        void DeletePostImage(Guid postId, string imagePath);
        void DeleteScheduledPostForUser(Guid userId);
    }
}

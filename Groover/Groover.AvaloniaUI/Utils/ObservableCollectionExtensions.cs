using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Utils
{
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// This method sorts a collection of GroupUsers by their roles using a comparer. The algortihm used is
        /// in-place insertion sort.
        /// </summary>
        /// <param name="groupUsers"></param>
        /// <param name="groupRoleComparer"></param>
        /// <param name="ascending"></param>
        public static void SortByRole(this ObservableCollection<GroupUserViewModel> groupUsers, 
            bool ascending = false)
        {
            if (ascending)
            {
                for (int i = 0; i < groupUsers.Count - 1; i++)
                {
                    int targetElemInd = i;
                    GroupUserViewModel targetElem = groupUsers[targetElemInd];

                    for (int j = i + 1; j < groupUsers.Count; j++)
                    {
                        GroupUserViewModel currentElem = groupUsers[j];
                        int compareValue = targetElem.CompareTo(currentElem);
                        if (compareValue > 0)
                        {
                            targetElemInd = j;
                            targetElem = currentElem;
                        }
                    }

                    if (targetElemInd != i)
                    {
                        groupUsers.Move(targetElemInd, i);
                        groupUsers.Move(i + 1, targetElemInd);
                    }
                }
            }
            else
            {
                for (int i = 0; i < groupUsers.Count - 1; i++)
                {
                    int targetElemInd = i;
                    GroupUserViewModel targetElem = groupUsers[targetElemInd];

                    for (int j = i + 1; j < groupUsers.Count; j++)
                    {
                        GroupUserViewModel currentElem = groupUsers[j];
                        int compareValue = targetElem.CompareTo(currentElem);
                        if (compareValue < 0)
                        {
                            targetElemInd = j;
                            targetElem = currentElem;
                        }
                    }

                    if (targetElemInd != i)
                    {
                        groupUsers.Move(targetElemInd, i);
                        groupUsers.Move(i + 1, targetElemInd);
                    }
                }
            }
        }

        /// <summary>
        /// This method will assume that the supplied collection is already sorted using the class comparer, and will insert
        /// the new group user accordingly.
        /// </summary>
        /// <param name="groupUsers"></param>
        /// <param name="newGroupUser"></param>
        /// <param name="groupRoleComparer"></param>
        /// <param name="ascending"></param>
        public static void InsertIntoSorted(this ObservableCollection<GroupUserViewModel> groupUsers,
            GroupUserViewModel newGroupUser,
            bool ascending = false)
        {
            if (ascending)
            {
                int i = 0;
                int compareValue;
                do
                {
                    compareValue = newGroupUser.CompareTo(groupUsers[i]);
                    i++;
                }
                while (compareValue < 0 &&
                       i < groupUsers.Count);


                //Found the spot
                if (compareValue >= 0)
                {
                    groupUsers.Insert(i - 1, newGroupUser);
                }
                else //Spot is at the end of the collection
                {
                    groupUsers.Insert(i, newGroupUser);
                }
            }
            else
            {
                int i = 0;
                int compareValue;
                do
                {
                    compareValue = newGroupUser.CompareTo(groupUsers[i]);
                    i++;
                }
                while (compareValue > 0 &&
                       i < groupUsers.Count);


                //Found the spot
                if (compareValue <= 0)
                {
                    groupUsers.Insert(i - 1, newGroupUser);
                }
                else //Spot is at the end of the collection
                {
                    groupUsers.Insert(i, newGroupUser);
                }
            }
        }
    }
}

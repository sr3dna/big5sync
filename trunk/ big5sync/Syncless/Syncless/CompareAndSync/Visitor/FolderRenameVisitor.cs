﻿using System.Collections.Generic;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Visitor
{
    public class FolderRenameVisitor : IVisitor
    {

        #region IVisitor Members

        public void Visit(FileCompareObject file, int numOfPaths)
        {
            //Do nothing
        }

        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            DetectFolderRename(folder, numOfPaths);
        }

        public void Visit(RootCompareObject root)
        {
            //Do nothing
        }

        #endregion

        private void DetectFolderRename(FolderCompareObject folder, int numOfPaths)
        {
            //Check that there is exactly one delete
            List<int> deletePos = new List<int>();
            for (int i = 0; i < numOfPaths; i++)
            {
                if (folder.ChangeType[i] == MetaChangeType.Delete)
                    deletePos.Add(i);
            }

            //if (deletePos.Count != 1)
            //{
            //    foreach (int i in deletePos)
            //        folder.ChangeType[i] = null;
            //    return;
            //}


            //1. If there exists a folder for which meta exists is true and exists is false, it is (aka changeType.delete)
            //highly probable that it is a folder rename
            //2. We check all folders which has the same meta name but different name as the non-existent folder
            //3. If the count is 1, we shall proceed to rename

            FolderCompareObject f = null;

            for (int i = 0; i < deletePos.Count /*numOfPaths*/; i++)
            {
                if (folder.ChangeType[deletePos[i]] == MetaChangeType.Delete)
                {
                    f = folder.Parent.GetRenamedFolder(folder.Name, folder.CreationTime[i], deletePos[i]);

                    if (f != null)
                    {
                        int counter = 0;

                        for (int j = 0; j < f.ChangeType.Length; j++)
                        {
                            if (f.ChangeType[j].HasValue && f.ChangeType[j] == MetaChangeType.New)
                                counter++;
                        }

                        if (counter != 1)
                        {
                            folder.ChangeType[deletePos[i]] = null;                         
                            return;
                        }

                        MergeRenamedFolder(folder, f, deletePos[i]);
                    }
                }
            }
        }

        //Merge the renamed folder into the folders with its old name, so that files are all compared.
        private void MergeRenamedFolder(FolderCompareObject actualFolder, FolderCompareObject renamedFolder, int pos)
        {
            Dictionary<string, BaseCompareObject>.KeyCollection renamedFolderContents = renamedFolder.Contents.Keys;
            BaseCompareObject o = null;
            FolderCompareObject actualFldrObj = null;
            FolderCompareObject renamedFolderObj = null;
            FileCompareObject actualFileObj = null;
            FileCompareObject renamedFileObj = null;

            actualFolder.NewName = renamedFolder.Name;
            actualFolder.ChangeType[pos] = MetaChangeType.Rename;

            foreach (string name in renamedFolderContents)
            {
                if (actualFolder.Contents.TryGetValue(name, out o))
                {

                    if ((actualFldrObj = o as FolderCompareObject) != null)
                    {
                        renamedFolderObj = renamedFolder.Contents[name] as FolderCompareObject;
                        actualFldrObj.ChangeType[pos] = renamedFolder.ChangeType[pos];
                        actualFldrObj.CreationTime[pos] = renamedFolder.CreationTime[pos];
                        actualFldrObj.Exists[pos] = renamedFolder.Exists[pos];
                        actualFldrObj.MetaCreationTime[pos] = renamedFolder.MetaCreationTime[pos];
                        actualFldrObj.MetaExists[pos] = renamedFolder.MetaExists[pos];
                        MergeRenamedFolder(actualFldrObj, renamedFolderObj, pos);
                    }
                    else
                    {
                        actualFileObj = o as FileCompareObject;
                        renamedFileObj = renamedFolder.Contents[name] as FileCompareObject;
                        actualFileObj.CreationTime[pos] = renamedFileObj.CreationTime[pos];
                        actualFileObj.Exists[pos] = renamedFileObj.Exists[pos];
                        actualFileObj.Hash[pos] = renamedFileObj.Hash[pos];
                        actualFileObj.LastWriteTime[pos] = renamedFileObj.LastWriteTime[pos];
                        actualFileObj.Length[pos] = renamedFileObj.Length[pos];
                        actualFileObj.MetaCreationTime[pos] = renamedFileObj.MetaCreationTime[pos];
                        actualFileObj.MetaExists[pos] = renamedFileObj.MetaExists[pos];
                        actualFileObj.MetaHash[pos] = renamedFileObj.MetaHash[pos];
                        actualFileObj.MetaLastWriteTime[pos] = renamedFileObj.MetaLastWriteTime[pos];
                        actualFileObj.MetaLength[pos] = renamedFileObj.MetaLength[pos];
                        actualFileObj.ChangeType[pos] = renamedFileObj.ChangeType[pos];
                    }
                }
                else
                {
                    actualFolder.AddChild(renamedFolder.Contents[name]);
                }
            }

            actualFolder.UpdateRename(pos);
            actualFolder.ChangeType[pos] = MetaChangeType.Rename;
            //actualFolder.Parent.Dirty = true; //EXP
            renamedFolder.Contents = new Dictionary<string, BaseCompareObject>();
            renamedFolder.Invalid = true;
        }
    }
}
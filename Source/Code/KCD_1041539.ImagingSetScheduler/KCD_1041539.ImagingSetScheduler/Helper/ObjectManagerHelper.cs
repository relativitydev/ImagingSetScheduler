using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KCD_1041539.ImagingSetScheduler.Context;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Interfaces.Workspace;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
    public class ObjectManagerHelper : IObjectManagerHelper
    {
        public readonly Guid IMAGING_SET_SCHEDULER = new Guid("45C9FEB9-43D8-43FE-A216-85B1F062B0A7");

        public async Task<RelativityObject> RetrieveSingleImagingSetScheduler(int workspaceArtifactId, IContextContainer contextContainer, int imagingSetSchedulerArtifactId)
        {
            if (workspaceArtifactId < 0)
            {
                throw new ArgumentException(Constant.ErrorMessages.WORKSPACE_ARTIFACT_ID_CANNOT_BE_NEGATIVE);
            }

            if (imagingSetSchedulerArtifactId < 0)
            {
                throw new ArgumentException(Constant.ErrorMessages.IMAGING_SET_SCHEDULER_ARTIFACT_ID_CANNOT_BE_NEGATIVE);
            }

            try
            {
                RelativityObject res;
                int batchSize = await contextContainer.InstanceSettingManager.GetIntegerValueAsync("Relativity.Imaging", "ImagingSetSchedulerBatchSize", 1000).ConfigureAwait(false);
                int totalCount;

                using (IObjectManager objectManager = contextContainer.ServicesProxyFactory.CreateServiceProxy<IObjectManager>())
                {
                    var queryRequest = NewQueryRequest($"(('Artifact ID' == {imagingSetSchedulerArtifactId}))");

                    QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, queryRequest, 1, batchSize).ConfigureAwait(false);
                    totalCount = queryResult.TotalCount;

                    if (totalCount > 1)
                    {
                        var errorContext = String.Format("An error has occurred: multiple instances of ImagingSetSchedulerArtifactId: {0}. WorkspaceArtifactId: {1}",
                            imagingSetSchedulerArtifactId, workspaceArtifactId);
                        throw new CustomExceptions.ImagingSetSchedulerException(errorContext);
                    }

                    if (totalCount == 0)
                    {
                        var errorContext =
                            String.Format("An error has occurred: no instance of ImagingSetSchedulerArtifactId: {0}. WorkspaceArtifactId: {1}",
                                imagingSetSchedulerArtifactId, workspaceArtifactId);
                        throw new CustomExceptions.ImagingSetSchedulerException(errorContext);
                    }

                    res = queryResult.Objects[0];
                    return res;
                }
            }
            catch(Exception ex)
            {
                var errorContext = String.Format("An error occurred when retrieving Imaging Set Scheduler [WorkspaceArtifactId: {0}, ImagingSetSchedulerArtifactId: {1}]",
                    workspaceArtifactId, imagingSetSchedulerArtifactId);
                throw new CustomExceptions.ImagingSetSchedulerException(errorContext, ex);
            }
        }

        public async Task<List<RelativityObject>> RetrieveAllImagingSetSchedulesNotWaitingAsync(int workspaceArtifactId, IContextContainer contextContainer)
        {
            int batchSize = await contextContainer.InstanceSettingManager.GetIntegerValueAsync("Relativity.Imaging", "ImagingSetSchedulerBatchSize", 1000).ConfigureAwait(false);
            int iterator = 1;
            int totalCount;
            List<RelativityObject> res = new List<RelativityObject>();

            using (IObjectManager objectManager = contextContainer.ServicesProxyFactory.CreateServiceProxy<IObjectManager>())
            {
                var queryRequest = NewQueryRequest("(((NOT 'Status' ISSET) OR NOT ('Status' IN ['waiting'])))");

                do
                {
                    QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, queryRequest, iterator, batchSize).ConfigureAwait(false);
                    totalCount = queryResult.TotalCount;
                    iterator += batchSize;
                    res.AddRange(queryResult.Objects);
                } while (iterator < totalCount);

                return res;
            }
        }
        public async Task<MassUpdateResult> UpdateImagingSetScheduler(int workspaceArtifactId, IContextContainer contextContainer, int imagingSetSchedulerArtifactId, DateTime? lastRun, DateTime? nextRun, string message, string status)
        {
            try
            {
                MassUpdateResult res;
                var fieldValuePairs = new List<FieldRefValuePair>
                {
                    new FieldRefValuePair()
                    {
                        Field = new FieldRef() { Guid = Constant.Guids.Field.ImagingSetScheduler.LAST_RUN },
                        Value = lastRun
                    },
                    new FieldRefValuePair()
                    {
                        Field = new FieldRef() { Guid = Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN },
                        Value = nextRun
                    },
                    new FieldRefValuePair()
                    {
                        Field = new FieldRef() { Guid = Constant.Guids.Field.ImagingSetScheduler.MESSAGES },
                        Value = message
                    },
                    new FieldRefValuePair()
                    {
                        Field = new FieldRef() { Guid = Constant.Guids.Field.ImagingSetScheduler.STATUS },
                        Value = status
                    }
                };

                var updateRequest = new MassUpdateByCriteriaRequest()
                {
                    ObjectIdentificationCriteria = new ObjectIdentificationCriteria
                    {
                        ObjectType = new ObjectTypeRef { Guid = IMAGING_SET_SCHEDULER },
                        Condition = $"(('Artifact ID' == {imagingSetSchedulerArtifactId}))"
                    },
                    FieldValues = fieldValuePairs,
                };

                using (IObjectManager objectManager =
                       contextContainer.ServicesProxyFactory.CreateServiceProxy<IObjectManager>())
                {
                    res =
                        await objectManager.UpdateAsync(workspaceArtifactId, updateRequest).ConfigureAwait(false);

                }

                return res;
            }
            catch (Exception ex)
            {
                var errorContext = String.Format(
                    "An error occurred when updating Imaging Set Scheduler [WorkspaceArtifactId: {0}, ImagingSetSchedulerArtifactId: {1}]",
                    workspaceArtifactId, imagingSetSchedulerArtifactId);
                throw new CustomExceptions.ImagingSetSchedulerException(errorContext, ex);
            }
        }

        private QueryRequest NewQueryRequest(string condition)
        {
            var queryRequest = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef { Guid = IMAGING_SET_SCHEDULER },
                Fields = new List<FieldRef>
                {
                    new FieldRef()
                    {
                        Guid = Constant.Guids.Field.ImagingSetScheduler.FREQUENCY
                    },
                    new FieldRef()
                    {
                        Guid = Constant.Guids.Field.ImagingSetScheduler.NAME
                    },
                    new FieldRef()
                    {
                        Guid = Constant.Guids.Field.ImagingSetScheduler.IMAGING_SET
                    },
                    new FieldRef()
                    {
                        Guid = Constant.Guids.Field.ImagingSetScheduler.TIME
                    },
                    new FieldRef()
                    {
                        Guid = Constant.Guids.Field.ImagingSetScheduler.LOCK_IMAGES_FOR_QC
                    },
                    new FieldRef()
                    {
                        Guid = Constant.Guids.Field.ImagingSetScheduler.LAST_RUN
                    },
                    new FieldRef()
                    {
                        Guid = Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN
                    },
                    new FieldRef()
                    {
                        Name = "System Created By"
                    }
                },
                Condition = condition
            };

            return queryRequest;
        }
    }
}

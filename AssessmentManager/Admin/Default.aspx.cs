using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.Data.SqlClient;

namespace AssessmentManager
{
    public partial class Default : System.Web.UI.Page
    {
        #region Member Variables

        private string m_ConnectionString = ConfigurationManager.ConnectionStrings["SqlServerConnectionString"].ConnectionString;

        private string m_aspnetUserId;
        private string m_aspnetUserName;
        private string m_aspnetUserEmail;

        private DataTable m_SurveyTable;
        private int m_SurveyId;
        private string m_SurveyTitle;

        private DataTable m_SectionTable;
        private int m_SectionId;
        private string m_SectionTitle;

        private DataTable m_SubsectionTable;
        private int m_SubsectionId;
        private string m_SubsectionTitle;

        private DataTable m_QuestionTable;
        private int m_QuestionId;
        private string m_QuestionTitle;

        private DataTable m_QuestionLibraryTable;

        private DataTable m_ResponseTable;
        private int m_ResponseId;
        private string m_ResponseTitle;

        private DataTable m_ResponseLibraryTable;

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                try
                {
                    m_SurveyId = Convert.ToInt32(Session["SurveyId"].ToString());
                }
                catch
                {
                }

                try
                {
                    m_SurveyTitle = Session["SurveyTitle"].ToString();
                }
                catch
                {
                }

                try
                {
                    m_SectionId = Convert.ToInt32(Session["SectionId"].ToString());
                }
                catch
                {
                }

                try
                {
                    m_SectionTitle = Session["SectionTitle"].ToString();
                }
                catch
                {
                }

                try
                {
                    m_SubsectionId = Convert.ToInt32(Session["SubsectionId"].ToString());
                }
                catch
                {
                }

                try
                {
                    m_SubsectionTitle = Session["SubsectionTitle"].ToString();
                }
                catch
                {
                }

                try
                {
                    m_QuestionId = Convert.ToInt32(Session["QuestionId"].ToString());
                }
                catch
                {
                }

                try
                {
                    m_QuestionTitle = Session["QuestionTitle"].ToString();
                }
                catch
                {
                }

                try
                {
                    m_ResponseId = Convert.ToInt32(Session["ResponseId"].ToString());
                }
                catch
                {
                }

                try
                {
                    m_ResponseTitle = Session["ResponseTitle"].ToString();
                }
                catch
                {
                }
            }
            else
            {
                GetSurvey();
            }

            GetUserInfo();
        }

        private void GetUserInfo()
        {
            MembershipUser myObject = Membership.GetUser();
            m_aspnetUserId = myObject.ProviderUserKey.ToString();
            m_aspnetUserName = myObject.UserName;
            m_aspnetUserEmail = myObject.Email;
        }

        private void HideAllPanels()
        {
            lblStatus.Text = string.Empty;

            panSurvey.Visible = false;
            panSection.Visible = false;
            panSubsection.Visible = false;
            panQuestion.Visible = false;
            panResponse.Visible = false;
        }

        #region Survey Section

        private void gridSurveyDataBind()
        {
            gridSurvey.DataSource = Session["SurveyTable"];
            gridSurvey.DataBind();
        }

        protected void gridSurvey_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gridSurvey.PageIndex = e.NewPageIndex;
            gridSurveyDataBind();
        }

        protected void gridSurvey_OnRowEditing(object sender, GridViewEditEventArgs e)
        {
            gridSurvey.EditIndex = e.NewEditIndex;
            gridSurveyDataBind();
        }

        protected void gridSurvey_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gridSurvey.EditIndex = -1;
            gridSurveyDataBind();
        }

        protected void gridSurvey_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            DataTable aSurveyTable = (DataTable)Session["SurveyTable"];

            //Update the values.
            GridViewRow row = gridSurvey.Rows[e.RowIndex];
            aSurveyTable.Rows[row.DataItemIndex]["Survey Id"] = row.Cells[1].Text;
            aSurveyTable.Rows[row.DataItemIndex]["Survey Title"] = ((TextBox)(row.Cells[2].Controls[0])).Text;

            string aSurveyId = row.Cells[1].Text;
            string aTitle = ((TextBox)(row.Cells[2].Controls[0])).Text;

            if (!String.IsNullOrEmpty(aTitle))
            {
                try
                {
                    using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                    {
                        using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_UpdateSurvey", aSqlConnection))
                        {
                            aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                            aSqlCommand.Parameters.AddWithValue("@SurveyId", aSurveyId);
                            aSqlCommand.Parameters.AddWithValue("@Title", aTitle);
                            aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                            aSqlCommand.Connection.Open();
                            aSqlCommand.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    string aMsg = ex.Message;
                    lblStatus.Text = "Error: " + aMsg;
                }
            }

            //Reset the edit index.
            gridSurvey.EditIndex = -1;
            gridSurveyDataBind();
        }

        private void GetSurvey()
        {
            panSurvey.Visible = true;
            lblStatus.Text = string.Empty;

            try
            {
                using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                {
                    using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_GetSurveyList", aSqlConnection))
                    {
                        aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        aSqlCommand.Connection.Open();

                        using (SqlDataAdapter aSqlDataAdapter = new SqlDataAdapter())
                        {
                            aSqlDataAdapter.SelectCommand = aSqlCommand;

                            using (SqlCommandBuilder aSqlCommandBuilder = new SqlCommandBuilder(aSqlDataAdapter))
                            {
                                DataSet ds = new DataSet("DataSet");
                                aSqlDataAdapter.Fill(ds, "DataSet");
                                m_SurveyTable = ds.Tables[0];

                                Session["SurveyTable"] = m_SurveyTable;
                                gridSurveyDataBind();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string aMsg = ex.Message;
                lblStatus.Text = "Error: " + aMsg;
            }
        }

        protected void gridSurvey_OnRowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            panSection.Visible = false;
            panSubsection.Visible = false;
            panQuestion.Visible = false;

            int aRow = e.RowIndex;
            int aSurveyId = Convert.ToInt32(gridSurvey.Rows[aRow].Cells[1].Text.ToString());

            using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
            {
                using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_DeleteSurvey", aSqlConnection))
                {
                    aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    aSqlCommand.Parameters.AddWithValue("@SurveyId", aSurveyId.ToString());
                    aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                    aSqlCommand.Connection.Open();
                    aSqlCommand.ExecuteNonQuery();
                }
            }

            GetSurvey();
        }

        protected void gridSurvey_OnRowSelecting(object sender, GridViewSelectEventArgs e)
        {
            panSection.Visible = false;
            panSubsection.Visible = false;
            panQuestion.Visible = false;

            int aRow = e.NewSelectedIndex;
            m_SurveyId = Convert.ToInt32(gridSurvey.Rows[aRow].Cells[1].Text.ToString());
            m_SurveyTitle = gridSurvey.Rows[aRow].Cells[2].Text.ToString();

            Session.Add("SurveyId", m_SurveyId);
            Session.Add("SurveyTitle", m_SurveyTitle);

            panSurvey.Visible = false;

            GetSection();
        }

        protected void btnAddSurvey_Click(object sender, EventArgs e)
        {
            panSection.Visible = false;
            panSubsection.Visible = false;
            panQuestion.Visible = false;

            lblAddSurvey.Visible = true;
            txtAddSurvey.Visible = true;
            btnAddSurveyOk.Visible = true;
            btnAddSurveyCancel.Visible = true;
        }

        protected void btnCloseSurvey_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Login.aspx");
        }

        protected void btnAddSurveyOk_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtAddSurvey.Text))
            {
                using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                {
                    using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_CreateSurvey", aSqlConnection))
                    {
                        aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        aSqlCommand.Parameters.AddWithValue("@SurveyTitle", txtAddSurvey.Text);
                        aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                        aSqlCommand.Connection.Open();
                        aSqlCommand.ExecuteNonQuery();
                    }
                }
            }


            txtAddSurvey.Text = string.Empty;

            lblAddSurvey.Visible = false;
            txtAddSurvey.Visible = false;
            btnAddSurveyOk.Visible = false;
            btnAddSurveyCancel.Visible = false;

            GetSurvey();
        }

        protected void btnAddSurveyCancel_Click(object sender, EventArgs e)
        {
            txtAddSurvey.Text = string.Empty;

            lblAddSurvey.Visible = false;
            txtAddSurvey.Visible = false;
            btnAddSurveyOk.Visible = false;
            btnAddSurveyCancel.Visible = false;

            GetSurvey();
        }
        #endregion

        #region Section Section

        private void gridSectionDataBind()
        {
            gridSection.DataSource = Session["SectionTable"];
            gridSection.DataBind();
        }

        protected void gridSection_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gridSection.PageIndex = e.NewPageIndex;
            gridSectionDataBind();
        }

        protected void gridSection_OnRowEditing(object sender, GridViewEditEventArgs e)
        {
            gridSection.EditIndex = e.NewEditIndex;
            gridSectionDataBind();
        }

        protected void gridSection_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gridSection.EditIndex = -1;
            gridSectionDataBind();
        }

        protected void gridSection_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            DataTable aSectionTable = (DataTable)Session["SectionTable"];

            //Update the values.
            GridViewRow row = gridSection.Rows[e.RowIndex];
            aSectionTable.Rows[row.DataItemIndex]["SectionId"] = row.Cells[1].Text;
            aSectionTable.Rows[row.DataItemIndex]["SectionTitle"] = ((TextBox)(row.Cells[2].Controls[0])).Text;

            string aSectionId = row.Cells[1].Text;
            string aSectionTitle = ((TextBox)(row.Cells[2].Controls[0])).Text;

            if (!String.IsNullOrEmpty(aSectionTitle))
            {
                try
                {
                    using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                    {
                        using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_UpdateSection", aSqlConnection))
                        {
                            aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                            aSqlCommand.Parameters.AddWithValue("@SectionId", aSectionId);
                            aSqlCommand.Parameters.AddWithValue("@SectionTitle", aSectionTitle);
                            aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                            aSqlCommand.Connection.Open();
                            aSqlCommand.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    string aMsg = ex.Message;
                    lblStatus.Text = "Error: " + aMsg;
                }
            }

            //Reset the edit index.
            gridSection.EditIndex = -1;
            gridSectionDataBind();
        }

        private void GetSection()
        {
            lblStatus.Text = string.Empty;

            try
            {
                using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                {
                    using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_GetSurvey", aSqlConnection))
                    {
                        aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        aSqlCommand.Parameters.AddWithValue("@SurveyId", m_SurveyId);
                        aSqlCommand.Connection.Open();

                        using (SqlDataAdapter aSqlDataAdapter = new SqlDataAdapter())
                        {
                            aSqlDataAdapter.SelectCommand = aSqlCommand;

                            using (SqlCommandBuilder aSqlCommandBuilder = new SqlCommandBuilder(aSqlDataAdapter))
                            {
                                DataSet ds = new DataSet("DataSet");
                                aSqlDataAdapter.Fill(ds, "DataSet");
                                m_SectionTable = ds.Tables[1];

                                Session["SectionTable"] = m_SectionTable;
                                gridSectionDataBind();

                                panSection.Visible = true;
                                lblSectionTitle.Text = string.Format("Sections For Survey: {0}", m_SurveyTitle);
                                lblSectionTitle.ToolTip = string.Format("{0} - SurveyId={1}", m_SurveyTitle, m_SurveyId.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string aMsg = ex.Message;
                lblStatus.Text = "Error: " + aMsg;
            }

        }

        protected void gridSection_OnRowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            panSubsection.Visible = false;
            panQuestion.Visible = false;

            int aRow = e.RowIndex;
            int aSectionId = Convert.ToInt32(gridSection.Rows[aRow].Cells[1].Text.ToString());

            using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
            {
                using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_DeleteSection", aSqlConnection))
                {
                    aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    aSqlCommand.Parameters.AddWithValue("@SurveyId", m_SurveyId.ToString());
                    aSqlCommand.Parameters.AddWithValue("@SectionId", aSectionId.ToString());
                    aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                    aSqlCommand.Connection.Open();
                    aSqlCommand.ExecuteNonQuery();
                }
            }

            GetSection();
        }

        protected void gridSection_OnRowSelecting(object sender, GridViewSelectEventArgs e)
        {
            HideAllPanels();

            int aRow = e.NewSelectedIndex;

            m_SectionId = Convert.ToInt32(gridSection.Rows[aRow].Cells[1].Text.ToString());
            m_SectionTitle = gridSection.Rows[aRow].Cells[2].Text.ToString();

            Session.Add("SectionId", m_SectionId);
            Session.Add("SectionTitle", m_SectionTitle);

            GetSubsection();
        }

        protected void btnAddSection_Click(object sender, EventArgs e)
        {
            panSubsection.Visible = false;
            panQuestion.Visible = false;

            lblAddSection.Visible = true;
            txtAddSection.Visible = true;
            btnAddSectionOk.Visible = true;
            btnAddSectionCancel.Visible = true;
        }

        protected void btnCloseSection_Click(object sender, EventArgs e)
        {
            HideAllPanels();

            panSurvey.Visible = true;
            GetSurvey();
        }

        protected void btnAddSectionOk_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtAddSection.Text))
            {
                using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                {
                    using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_CreateSection", aSqlConnection))
                    {
                        aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        aSqlCommand.Parameters.AddWithValue("@SurveyId", m_SurveyId);
                        aSqlCommand.Parameters.AddWithValue("@SectionTitle", txtAddSection.Text);
                        aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                        aSqlCommand.Connection.Open();
                        aSqlCommand.ExecuteNonQuery();
                    }
                }
            }

            txtAddSection.Text = string.Empty;

            lblAddSection.Visible = false;
            txtAddSection.Visible = false;
            btnAddSectionOk.Visible = false;
            btnAddSectionCancel.Visible = false;

            GetSection();
        }

        protected void btnAddSectionCancel_Click(object sender, EventArgs e)
        {
            txtAddSection.Text = string.Empty;

            lblAddSection.Visible = false;
            txtAddSection.Visible = false;
            btnAddSectionOk.Visible = false;
            btnAddSectionCancel.Visible = false;

            GetSection();
        }

        #endregion

        #region Subsection Section

        private void gridSubsectionDataBind()
        {
            gridSubsection.DataSource = Session["SubsectionTable"];
            gridSubsection.DataBind();
        }

        protected void gridSubsection_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gridSubsection.PageIndex = e.NewPageIndex;
            gridSectionDataBind();
        }

        protected void gridSubsection_OnRowEditing(object sender, GridViewEditEventArgs e)
        {
            gridSubsection.EditIndex = e.NewEditIndex;
            gridSubsectionDataBind();
        }

        protected void gridSubsection_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gridSubsection.EditIndex = -1;
            gridSubsectionDataBind();
        }

        protected void gridSubsection_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            DataTable aSubsectionTable = (DataTable)Session["SubsectionTable"];

            //Update the values.
            GridViewRow row = gridSubsection.Rows[e.RowIndex];
            aSubsectionTable.Rows[row.DataItemIndex]["SubsectionId"] = row.Cells[1].Text;
            aSubsectionTable.Rows[row.DataItemIndex]["SubsectionTitle"] = ((TextBox)(row.Cells[2].Controls[0])).Text;

            string aSubsectionId = row.Cells[1].Text;
            string aSubsectionTitle = ((TextBox)(row.Cells[2].Controls[0])).Text;

            if (!String.IsNullOrEmpty(aSubsectionTitle))
            {
                try
                {
                    using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                    {
                        using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_UpdateSubsection", aSqlConnection))
                        {
                            aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                            aSqlCommand.Parameters.AddWithValue("@SubsectionId", aSubsectionId);
                            aSqlCommand.Parameters.AddWithValue("@SubsectionTitle", aSubsectionTitle);
                            aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                            aSqlCommand.Connection.Open();
                            aSqlCommand.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    string aMsg = ex.Message;
                    lblStatus.Text = "Error: " + aMsg;
                }
            }

            //Reset the edit index.
            gridSubsection.EditIndex = -1;
            gridSubsectionDataBind();
        }

        private void GetSubsection()
        {
            lblStatus.Text = string.Empty;

            try
            {
                using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                {
                    using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_GetSubsection", aSqlConnection))
                    {
                        aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        aSqlCommand.Parameters.AddWithValue("@SectionId", m_SectionId);
                        aSqlCommand.Connection.Open();

                        using (SqlDataAdapter aSqlDataAdapter = new SqlDataAdapter())
                        {
                            aSqlDataAdapter.SelectCommand = aSqlCommand;

                            using (SqlCommandBuilder aSqlCommandBuilder = new SqlCommandBuilder(aSqlDataAdapter))
                            {
                                DataSet ds = new DataSet("DataSet");
                                aSqlDataAdapter.Fill(ds, "DataSet");
                                m_SubsectionTable = ds.Tables[0];

                                Session["SubsectionTable"] = m_SubsectionTable;
                                gridSubsectionDataBind();

                                panSubsection.Visible = true;

                                lblSubsectionTitle.Text = string.Format("Subsections For Section: {0}", m_SectionTitle);
                                lblSubsectionTitle.ToolTip = string.Format("{0} - SectionId={1}", m_SectionTitle, m_SectionId.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string aMsg = ex.Message;
                lblStatus.Text = "Error: " + aMsg;
            }
        }

        protected void gridSubsection_OnRowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            panQuestion.Visible = false;

            int aRow = e.RowIndex;
            int aSubSectionId = Convert.ToInt32(gridSubsection.Rows[aRow].Cells[1].Text.ToString());

            using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
            {
                using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_DeleteSubsection", aSqlConnection))
                {
                    aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    aSqlCommand.Parameters.AddWithValue("@SectionId", m_SectionId.ToString());
                    aSqlCommand.Parameters.AddWithValue("@SubsectionId", aSubSectionId.ToString());
                    aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                    aSqlCommand.Connection.Open();
                    aSqlCommand.ExecuteNonQuery();
                }
            }

            GetSubsection();
        }

        protected void gridSubsection_OnRowSelecting(object sender, GridViewSelectEventArgs e)
        {
            HideAllPanels();

            int aRow = e.NewSelectedIndex;

            m_SubsectionId = Convert.ToInt32(gridSubsection.Rows[aRow].Cells[1].Text.ToString());
            m_SubsectionTitle = gridSubsection.Rows[aRow].Cells[2].Text.ToString();

            Session.Add("SubsectionId", m_SubsectionId);
            Session.Add("SubsectionTitle", m_SubsectionTitle);

            GetQuestion();
        }

        protected void btnAddSubsection_Click(object sender, EventArgs e)
        {
            panQuestion.Visible = false;

            lblAddSubsection.Visible = true;
            txtAddSubsection.Visible = true;
            btnAddSubsectionOk.Visible = true;
            btnAddSubsectionCancel.Visible = true;
        }

        protected void btnCloseSubsection_Click(object sender, EventArgs e)
        {
            HideAllPanels();
            GetSection();
        }

        protected void btnAddSubsectionOk_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtAddSubsection.Text))
            {
                using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                {
                    using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_CreateSubsection", aSqlConnection))
                    {
                        aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        aSqlCommand.Parameters.AddWithValue("@SectionId", m_SectionId);
                        aSqlCommand.Parameters.AddWithValue("@SubsectionTitle", txtAddSubsection.Text);
                        aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                        aSqlCommand.Connection.Open();
                        aSqlCommand.ExecuteNonQuery();
                    }
                }
            }

            txtAddSubsection.Text = string.Empty;

            lblAddSubsection.Visible = false;
            txtAddSubsection.Visible = false;
            btnAddSubsectionOk.Visible = false;
            btnAddSubsectionCancel.Visible = false;

            GetSubsection();
        }

        protected void btnAddSubsectionCancel_Click(object sender, EventArgs e)
        {
            txtAddSubsection.Text = string.Empty;

            lblAddSubsection.Visible = false;
            txtAddSubsection.Visible = false;
            btnAddSubsectionOk.Visible = false;
            btnAddSubsectionCancel.Visible = false;

            GetSubsection();
        }
        #endregion

        #region Question Section
        private void gridQuestionDataBind()
        {
            gridQuestion.DataSource = Session["QuestionTable"];
            gridQuestion.DataBind();
        }

        protected void gridQuestion_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gridQuestion.PageIndex = e.NewPageIndex;
            gridQuestionDataBind();
        }

        protected void gridQuestion_OnRowEditing(object sender, GridViewEditEventArgs e)
        {
            gridQuestion.EditIndex = e.NewEditIndex;
            gridQuestionDataBind();
        }

        protected void gridQuestion_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gridQuestion.EditIndex = -1;
            gridQuestionDataBind();
        }

        protected void gridQuestion_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            DataTable aQuestionTable = (DataTable)Session["QuestionTable"];

            //Update the values.
            GridViewRow row = gridQuestion.Rows[e.RowIndex];
            aQuestionTable.Rows[row.DataItemIndex]["QuestionId"] = row.Cells[1].Text;
            aQuestionTable.Rows[row.DataItemIndex]["Question"] = ((TextBox)(row.Cells[2].Controls[0])).Text;

            int aQuestionOrder = 0;
            try
            {
                aQuestionOrder = Convert.ToInt32(((TextBox)(row.Cells[3].Controls[0])).Text);
            }
            catch
            {
                aQuestionOrder = 0;
            }
            finally
            {
                aQuestionTable.Rows[row.DataItemIndex]["Order"] = aQuestionOrder;
            }

            string aQuestionId = row.Cells[1].Text;
            string aQuestionTitle = ((TextBox)(row.Cells[2].Controls[0])).Text;

            if (!String.IsNullOrEmpty(aQuestionTitle))
            {
                try
                {
                    using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                    {
                        using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_UpdateQuestion", aSqlConnection))
                        {
                            aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                            aSqlCommand.Parameters.AddWithValue("@SubsectionId", m_SubsectionId);
                            aSqlCommand.Parameters.AddWithValue("@QuestionId", aQuestionId);
                            aSqlCommand.Parameters.AddWithValue("@QuestionTitle", aQuestionTitle);
                            aSqlCommand.Parameters.AddWithValue("@QuestionOrder", aQuestionOrder.ToString());
                            aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                            aSqlCommand.Connection.Open();
                            aSqlCommand.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    string aMsg = ex.Message;
                    lblStatus.Text = "Error: " + aMsg;
                }
            }
            else
            {
                lblStatus.Text = "Error: The question text may not be blank.";
            }

            //Reset the edit index.
            gridQuestion.EditIndex = -1;
            gridQuestionDataBind();
        }

        private void GetQuestion()
        {
            lblStatus.Text = string.Empty;

            try
            {
                using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                {
                    using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_GetQuestion", aSqlConnection))
                    {
                        aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        aSqlCommand.Parameters.AddWithValue("@SubsectionId", m_SubsectionId);
                        aSqlCommand.Connection.Open();

                        using (SqlDataAdapter aSqlDataAdapter = new SqlDataAdapter())
                        {
                            aSqlDataAdapter.SelectCommand = aSqlCommand;

                            using (SqlCommandBuilder aSqlCommandBuilder = new SqlCommandBuilder(aSqlDataAdapter))
                            {
                                DataSet ds = new DataSet("DataSet");
                                aSqlDataAdapter.Fill(ds, "DataSet");
                                m_QuestionTable = ds.Tables[0];

                                Session["QuestionTable"] = m_QuestionTable;
                                gridQuestionDataBind();

                                panQuestion.Visible = true;

                                lblQuestionTitle.Text = string.Format("Questions For Subsection: {0}", m_SubsectionTitle);
                                lblQuestionTitle.ToolTip = string.Format("{0} - SubsectionId={1}", m_SubsectionTitle, m_SubsectionId.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string aMsg = ex.Message;
                lblStatus.Text = "Error: " + aMsg;
            }
        }

        protected void gridQuestion_OnRowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            lblStatus.Text = string.Empty;
            panQuestion.Visible = false;

            int aRow = e.RowIndex;
            int aQuestionId = Convert.ToInt32(gridQuestion.Rows[aRow].Cells[1].Text.ToString());

            try
            {
                using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                {
                    using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_DeleteQuestion", aSqlConnection))
                    {
                        aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        aSqlCommand.Parameters.AddWithValue("@SubsectionId", m_SubsectionId.ToString());
                        aSqlCommand.Parameters.AddWithValue("@QuestionId", aQuestionId.ToString());
                        aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                        aSqlCommand.Connection.Open();
                        aSqlCommand.ExecuteNonQuery();
                    }
                }

                GetQuestion();
            }
            catch (Exception ex)
            {
                string aMsg = ex.Message;
                lblStatus.Text = "Error: " + aMsg;
            }
        }

        protected void gridQuestion_OnRowSelecting(object sender, GridViewSelectEventArgs e)
        {
            HideAllPanels();

            int aRow = e.NewSelectedIndex;

            m_QuestionId = Convert.ToInt32(gridQuestion.Rows[aRow].Cells[1].Text.ToString());
            m_QuestionTitle = gridQuestion.Rows[aRow].Cells[2].Text.ToString();

            Session.Add("QuestionId", m_QuestionId);
            Session.Add("QuestionTitle", m_QuestionTitle);

            GetResponse();
        }

        protected void btnAddQuestion_Click(object sender, EventArgs e)
        {
            lblAddQuestion.Visible = true;
            txtAddQuestion.Visible = true;
            btnAddQuestionOk.Visible = true;
            btnAddQuestionCancel.Visible = true;

            responsesGrid.Visible = true;

            try
            {
                using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                {
                    using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_GetResponseList", aSqlConnection))
                    {
                        aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        aSqlCommand.Connection.Open();

                        using (SqlDataAdapter aSqlDataAdapter = new SqlDataAdapter())
                        {
                            aSqlDataAdapter.SelectCommand = aSqlCommand;

                            using (SqlCommandBuilder aSqlCommandBuilder = new SqlCommandBuilder(aSqlDataAdapter))
                            {
                                DataSet ds = new DataSet("DataSet");
                                aSqlDataAdapter.Fill(ds, "DataSet");
                                responsesGrid.DataSource = ds.Tables[0];
                                responsesGrid.DataBind();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string aMsg = ex.Message;
                lblStatus.Text = "Error: " + aMsg;
            }

        }

        protected void btnCloseQuestion_Click(object sender, EventArgs e)
        {
            HideAllPanels();
            GetSubsection();
        }

        protected void btnAddQuestionOk_Click(object sender, EventArgs e)
        {
            int NewQuestionId=-1;

            if (!String.IsNullOrEmpty(txtAddQuestion.Text))
            {
                using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                {
                    using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_CreateQuestion", aSqlConnection))
                    {
                        aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        aSqlCommand.Parameters.AddWithValue("@SubSectionId", m_SubsectionId);
                        aSqlCommand.Parameters.AddWithValue("@Question", txtAddQuestion.Text);
                        aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                        aSqlCommand.Connection.Open();

                        object value = aSqlCommand.ExecuteScalar();
                        NewQuestionId = Convert.ToInt32(value);
                    }
                }
            }

            if (NewQuestionId != -1)
            {
                // Loop through responses to find selected ones.
                foreach (GridViewRow r in responsesGrid.Rows)
                {
                    CheckBox cb = new CheckBox();
                    cb = (CheckBox)r.FindControl("addCheckBox");

                    if (cb != null && cb.Checked == true)
                    {
                        int ResponseId = Convert.ToInt32(r.Cells[1].Text.ToString());

                        using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                        {
                            using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_AddExistingResponseToQuestion", aSqlConnection))
                            {
                                aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                                aSqlCommand.Parameters.AddWithValue("@QuestionId", NewQuestionId.ToString());
                                aSqlCommand.Parameters.AddWithValue("@ResponseId", ResponseId.ToString());
                                aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                                aSqlCommand.Connection.Open();
                                aSqlCommand.ExecuteNonQuery();
                            }
                        }

                    }
                }
            }

            txtAddQuestion.Text = string.Empty;

            lblAddQuestion.Visible = false;
            txtAddQuestion.Visible = false;
            btnAddQuestionOk.Visible = false;
            btnAddQuestionCancel.Visible = false;

            responsesGrid.Visible = false;

            GetQuestion();
        }

        protected void btnAddQuestionCancel_Click(object sender, EventArgs e)
        {
            txtAddQuestion.Text = string.Empty;

            lblAddQuestion.Visible = false;
            txtAddQuestion.Visible = false;
            btnAddQuestionOk.Visible = false;
            btnAddQuestionCancel.Visible = false;

            GetSection();
        }

        protected void btnSelectQuestion_Click(object sender, EventArgs e)
        {
            //HideAllPanels();
            //GetQuestionList();
        }
        #endregion

        #region Response
        private void gridResponseDataBind()
        {
            gridResponse.DataSource = Session["ResponseTable"];
            gridResponse.DataBind();
        }

        protected void gridResponse_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gridResponse.PageIndex = e.NewPageIndex;
            gridResponseDataBind();
        }

        protected void gridResponse_OnRowEditing(object sender, GridViewEditEventArgs e)
        {
            gridResponse.EditIndex = e.NewEditIndex;
            gridResponseDataBind();
        }

        protected void gridResponse_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gridResponse.EditIndex = -1;
            gridResponseDataBind();
        }

        protected void gridResponse_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            DataTable aResponseTable = (DataTable)Session["ResponseTable"];

            //Update the values.
            GridViewRow row = gridResponse.Rows[e.RowIndex];
            aResponseTable.Rows[row.DataItemIndex]["ResponseId"] = row.Cells[1].Text;
            aResponseTable.Rows[row.DataItemIndex]["Response"] = ((TextBox)(row.Cells[2].Controls[0])).Text;

            string aResponseId = row.Cells[1].Text;
            string aResponseTitle = ((TextBox)(row.Cells[2].Controls[0])).Text;

            if (!String.IsNullOrEmpty(aResponseTitle))
            {
                try
                {
                    using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                    {
                        using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_UpdateResponse", aSqlConnection))
                        {
                            aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                            aSqlCommand.Parameters.AddWithValue("@QuestionId", m_QuestionId);
                            aSqlCommand.Parameters.AddWithValue("@ResponseId", aResponseId);
                            aSqlCommand.Parameters.AddWithValue("@Response", aResponseTitle);
                            aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                            aSqlCommand.Connection.Open();
                            aSqlCommand.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    string aMsg = ex.Message;
                    lblStatus.Text = "Error: " + aMsg;
                }
            }
            else
            {
                lblStatus.Text = "Error: The Response text may not be blank.";
            }

            //Reset the edit index.
            gridResponse.EditIndex = -1;
            gridResponseDataBind();
        }

        private void GetResponse()
        {
            lblStatus.Text = string.Empty;

            using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
            {
                using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_GetResponse", aSqlConnection))
                {
                    aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    aSqlCommand.Parameters.AddWithValue("@QuestionId", m_QuestionId);
                    aSqlCommand.Connection.Open();

                    using (SqlDataAdapter aSqlDataAdapter = new SqlDataAdapter())
                    {
                        aSqlDataAdapter.SelectCommand = aSqlCommand;

                        using (SqlCommandBuilder aSqlCommandBuilder = new SqlCommandBuilder(aSqlDataAdapter))
                        {
                            DataSet ds = new DataSet("DataSet");
                            aSqlDataAdapter.Fill(ds, "DataSet");
                            m_ResponseTable = ds.Tables[0];

                            Session["ResponseTable"] = m_ResponseTable;
                            gridResponseDataBind();

                            panResponse.Visible = true;

                            lblResponseTitle.Text = string.Format("Responses For Question: {0}", m_QuestionTitle);
                            lblResponseTitle.ToolTip = string.Format("{0} - QuestionId={1}", m_QuestionTitle, m_QuestionId.ToString());

                        }
                    }
                }
            }
        }

        protected void gridResponse_OnRowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            panResponse.Visible = false;

            int aRow = e.RowIndex;
            int aResponseId = Convert.ToInt32(gridResponse.Rows[aRow].Cells[1].Text.ToString());

            using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
            {
                using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_DeleteResponse", aSqlConnection))
                {
                    aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    aSqlCommand.Parameters.AddWithValue("@QuestionId", m_QuestionId.ToString());
                    aSqlCommand.Parameters.AddWithValue("@ResponseId", aResponseId.ToString());
                    aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                    aSqlCommand.Connection.Open();
                    aSqlCommand.ExecuteNonQuery();
                }
            }

            GetResponse();
        }

        protected void gridResponse_OnRowSelecting(object sender, GridViewSelectEventArgs e)
        {
            HideAllPanels();

            int aRow = e.NewSelectedIndex;

            m_ResponseId = Convert.ToInt32(gridResponse.Rows[aRow].Cells[1].Text.ToString());
            m_ResponseTitle = gridResponse.Rows[aRow].Cells[2].Text.ToString();

            Session.Add("PossibleResponseId", m_ResponseId);
            Session.Add("PossibleResponse", m_ResponseTitle);

            GetResponse();
        }

        protected void btnAddResponse_Click(object sender, EventArgs e)
        {
            lblAddResponse.Visible = true;
            txtAddResponse.Visible = true;
            btnAddResponseOk.Visible = true;
            btnAddResponseCancel.Visible = true;
        }

        protected void btnCloseResponse_Click(object sender, EventArgs e)
        {
            HideAllPanels();
            GetQuestion();
        }

        protected void btnAddResponseOk_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtAddResponse.Text))
            {
                using (SqlConnection aSqlConnection = new SqlConnection(m_ConnectionString))
                {
                    using (SqlCommand aSqlCommand = new SqlCommand("sp_amis_admin_CreateResponse", aSqlConnection))
                    {
                        aSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        aSqlCommand.Parameters.AddWithValue("@QuestionId", m_QuestionId);
                        aSqlCommand.Parameters.AddWithValue("@Response", txtAddResponse.Text);
                        aSqlCommand.Parameters.AddWithValue("@aspnetUserId", m_aspnetUserId);
                        aSqlCommand.Connection.Open();
                        aSqlCommand.ExecuteNonQuery();
                    }
                }
            }

            txtAddResponse.Text = string.Empty;

            lblAddResponse.Visible = false;
            txtAddResponse.Visible = false;
            btnAddResponseOk.Visible = false;
            btnAddResponseCancel.Visible = false;

            GetResponse();
        }

        protected void btnAddResponseCancel_Click(object sender, EventArgs e)
        {
            txtAddResponse.Text = string.Empty;

            lblAddResponse.Visible = false;
            txtAddResponse.Visible = false;
            btnAddResponseOk.Visible = false;
            btnAddResponseCancel.Visible = false;

            GetSection();
        }

        protected void btnSelectResponse_Click(object sender, EventArgs e)
        {
            //HideAllPanels();
            //GetResponseList();
        }
        #endregion     

        protected void responsesGrid_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

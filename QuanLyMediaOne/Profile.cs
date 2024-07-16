using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QuanLyLinhKienDIenTu.Login;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;

namespace QuanLyLinhKienDIenTu
{
    public partial class Profile : Form
    {
        string strCon = Connecting.GetConnectionString();
        public MainForm mf;
        int userId = UserSession.UserId;
        string role = UserSession.role;

        bool isAddMode = AddUser.IsAddMode;

        public Profile(MainForm mainForm)
        {
            InitializeComponent();
            mf = mainForm;
            Round.SetRoundedButton(btnFix);
            Round.SetSharpCornerPanel(panel_fix);
            txtNumber.KeyPress += TxtNumber_KeyPress;
            if(role == "Khach Hang")
            {
                label6.Visible = false;
                label2.Text = "Mã Khách Hàng";
            }
        }

        private void TxtNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Kiểm tra xem phím được nhấn có phải là số không
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                // Nếu không phải số, hủy sự kiện KeyPress
                e.Handled = true;
            }
        }

        private void Profile_Load(object sender, EventArgs e)
        {
            this.ControlBox = false;
            HideTextBox();
            DisplayProfile(userId);
        }

        //láy thông tin từ database
        private void DisplayProfile(int userId)
        {
            string query = @"
                  SELECT e.MANV AS MA, e.HO, e.TEN, e.EMAIL, e.SDT, e.DIACHI, e.LUONG, e.GIOITINH, u.VAITRO
                  FROM NHANVIEN e
                  INNER JOIN NGUOIDUNG u ON e.MAND = u.MAND 
                  WHERE e.MAND = @userId
                  UNION
                  SELECT e.MAKH AS MA, e.HO, e.TEN, e.EMAIL, e.SDT, e.DIACHI, NULL AS LUONG, e.GIOITINH, u.VAITRO
                  FROM KHACHHANG e
                  INNER JOIN NGUOIDUNG u ON e.MAND = u.MAND 
                  WHERE e.MAND = @userId";

            using (SqlConnection connection = new SqlConnection(strCon))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {

                        LbMaNV.Text = reader["MA"].ToString();
                        txtName.Text = reader["HO"].ToString()+ " " + reader["TEN"].ToString();
                        cboGender.Text = reader["GIOITINH"].ToString();
                        txtNumber.Text = reader["SDT"].ToString();
                        txtEmail.Text = reader["EMAIL"].ToString();
                        txtAdress.Text = reader["DIACHI"].ToString();
                        TxtSalary.Text = reader["LUONG"].ToString();
                        string role = reader["VAITRO"].ToString();
                    }
                }
            }
        }

        //Ẩn textbox chỉ hiện nội dung
        private void HideTextBox()
        {
            //Xóa viền
            txtName.BorderStyle = BorderStyle.None;
            txtNumber.BorderStyle = BorderStyle.None;
            txtEmail.BorderStyle = BorderStyle.None;
            txtAdress.BorderStyle = BorderStyle.None;
            TxtSalary.BorderStyle = BorderStyle.None;

            //Vô hiệu hóa nút
            txtName.Enabled = false;
            cboGender.Enabled = false;
            txtNumber.Enabled = false;
            txtEmail.Enabled = false;
            txtAdress.Enabled = false;
            TxtSalary.Enabled = false;

            //Đặt nền trong suất
            txtName.BackColor = this.BackColor;
            cboGender.BackColor = this.BackColor;
            TxtSalary.BackColor = this.BackColor;
            txtNumber.BackColor = this.BackColor;
            txtEmail.BackColor = this.BackColor;
            txtAdress.BackColor = this.BackColor;

        }
        
        //Hiện thị lại Textbox
        private void ShowTextBox()
        {
            //Hiện viền
            txtName.BorderStyle = BorderStyle.FixedSingle;
            txtNumber.BorderStyle = BorderStyle.FixedSingle;
            txtEmail.BorderStyle = BorderStyle.FixedSingle;
            txtAdress.BorderStyle = BorderStyle.FixedSingle;

            //Vô hiệu hóa nút
            txtName.Enabled = true;
            cboGender.Enabled = true;
            txtNumber.Enabled = true;
            txtEmail.Enabled = true;
            txtAdress.Enabled = true;

            // Đặt lại màu nền mặc định
            txtName.BackColor = SystemColors.Window;
            cboGender.BackColor = SystemColors.Window;
            txtNumber.BackColor = SystemColors.Window;
            txtEmail.BackColor = SystemColors.Window;
            txtAdress.BackColor = SystemColors.Window;
        }

        private void txtHireDate_TextChanged(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        bool isEditMode = true;
        private void btnFix_Click(object sender, EventArgs e)
        {
            if (isEditMode)
            {
                FixMode();
            }
            else
            {
                ViewMode();
            }

        }

        //chế độ sữa 
        private void FixMode()
        {
            ShowTextBox();
            isEditMode = false;
            btnFix.Text = "     Lưu";
            btnFix.Image = Image.FromFile(@"D:\Code\SQL\QuanLyLinhKienMayTinh\Icon\diskette.png");
        }

        //chế độ xem và lữu
        private void ViewMode()
        {
            UpdateProfileInfo();
            DisplayProfile(UserSession.UserId);
            HideTextBox();
            isEditMode = true;
            btnFix.Text = "     Sữa";
            MainForm.Instance.UpdateProfilePicture(UserSession.UserId);
            btnFix.Image = Image.FromFile(@"D:\Code\SQL\QuanLyLinhKienMayTinh\Icon\service.png");
        }

        //Cập nhập thông tin cá nhân
        private void UpdateProfileInfo()
        {

            string fullName = txtName.Text;
            string gender = cboGender.SelectedItem != null ? cboGender.SelectedItem.ToString() : ""; // Kiểm tra null
            string phoneNumber = txtNumber.Text;
            string email = txtEmail.Text;
            string address = txtAdress.Text;

            // Cập nhật thông tin vào cơ sở dữ liệu
            string query;
            if(role =="Khach Hang")
            {
                query = @"
                UPDATE KHACHHANG
                SET GIOITINH = @gender,
					SDT = @phoneNumber,
					EMAIL = @email,
					DIACHI = @address
                WHERE MAND = @userId";
            }
            else
            {
                query = @"
				UPDATE NHANVIEN
                SET GIOITINH = @gender,
					SDT = @phoneNumber,
					EMAIL = @email,
					DIACHI = @address
                WHERE MAND = @userId";
            }

            using (SqlConnection connection = new SqlConnection(strCon))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Thêm các tham số
                    command.Parameters.AddWithValue("@gender", gender);
                    command.Parameters.AddWithValue("@phoneNumber", phoneNumber);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@address", address);
                    command.Parameters.AddWithValue("@userId", UserSession.UserId);

                    // Mở kết nối và thực thi truy vấn
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        MessageBox.Show("Thông tin hồ sơ đã được cập nhật thành công.", "Cập nhật thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Đã xảy ra lỗi khi cập nhật thông tin hồ sơ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void txtGender_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtGender_Click(object sender, EventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Click(object sender, EventArgs e)
        {
            OpenChildChildForm(new Payment());
        }

        private void OpenChildChildForm(Form childForm)
        {
            if (mf != null)
            {
                mf.OpenChildChildForm(childForm); // Gọi phương thức của MainForm từ tham chiếu
            }
        }

        private void txtNumber_TextChanged(object sender, EventArgs e)
        {

        }

        //mở form salary
        private void pSalary_Click(object sender, EventArgs e)
        {
            OpenChildChildForm(new Salary());
        }

        private void pSalary_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {

        }

        private void TxtSalary_TextChanged(object sender, EventArgs e)
        {

        }
    }

}

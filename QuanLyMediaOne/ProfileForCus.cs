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
    public partial class ProfileForCus : Form
    {
        string strCon = Connecting.GetConnectionString();
        string imagePath;
        public MainForm mf;
        bool isAddMode = AddUser.IsAddMode;
        int userId = UserSession.UserId;
        string role = UserSession.role;
        int CusID = CustomerID.userId;
        bool CustomeInfo = DanhSachKhachHang.CustomerInfo;

        public ProfileForCus(MainForm mainForm)
        {
            InitializeComponent();
            mf = mainForm;
            Round.SetRoundedButton(btnFix);
            Round.SetSharpCornerPanel(panel_fix);
            txtAge.KeyPress += TxtAge_KeyPress;
            txtNumber.KeyPress += TxtNumber_KeyPress;
        }

        private void TxtNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                // Từ chối ký tự nhập vào bằng cách hủy sự kiện
                e.Handled = true;
            }
        }

        private void TxtAge_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                // Nếu không phải là một số hoặc phím Backspace, loại bỏ ký tự đó
                e.Handled = true;
            }
        }

        bool isEditMode = true;
        private void Profile_Load(object sender, EventArgs e)
        {
            this.ControlBox = false;
            if (CustomeInfo)
            {
                DisplayProfile(CusID);
                HideTextBox();
                CustomeInfo = false;
                isEditMode = true;
            }
            else if (isAddMode)
            {
                isAddMode = false;
                FixMode();
                isEditMode = false;
            }

            if (role == "Nhan vien")
            {
                panel_fix.Visible = false;
            }
        }

        private void btnFix_Click(object sender, EventArgs e)
        {
            if (isEditMode)
            {
                FixMode();
                isEditMode = false;
            }
            else
            {
                ViewMode();
                isEditMode = true;
            }

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
                        txtName.Text = reader["HO"].ToString() + " " + reader["TEN"].ToString();
                        cboGender.Text = reader["GIOITINH"].ToString();
                        txtNumber.Text = reader["SDT"].ToString();
                        txtEmail.Text = reader["EMAIL"].ToString();
                        txtAdress.Text = reader["DIACHI"].ToString();
                    }
                }
            }
        }

        //Sữa thông tin
        private void UpdateProfileInfo()
        {


            string fullName = txtName.Text;
            string gender = cboGender.SelectedItem != null ? cboGender.SelectedItem.ToString() : "Khác"; // Kiểm tra null
            string phoneNumber = txtNumber.Text != null ? txtNumber.Text : null;
            string email = txtEmail.Text;
            string address = txtAdress.Text;

            // Cập nhật thông tin vào cơ sở dữ liệu
            string query = @"
               UPDATE KHACHHANG
                SET TEN = @fullName,
                    GIOITINH = @gender,
					SDT = @phoneNumber,
					EMAIL = @email,
					DIACHI = @address
                WHERE MAND = @userId";

            using (SqlConnection connection = new SqlConnection(strCon))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Thêm các tham số
                    command.Parameters.AddWithValue("@fullName", fullName);
                    command.Parameters.AddWithValue("@gender", gender);
                    command.Parameters.AddWithValue("@phoneNumber", phoneNumber);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@address", address);
                    command.Parameters.AddWithValue("@userId", CusID);


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

        //Ẩn textbox chỉ hiện nội dung
        private void HideTextBox()
        {
            //Xóa viền
            txtName.BorderStyle = BorderStyle.None;
            txtAge.BorderStyle = BorderStyle.None;
            txtNumber.BorderStyle = BorderStyle.None;
            txtEmail.BorderStyle = BorderStyle.None;
            txtAdress.BorderStyle = BorderStyle.None;

            //Vô hiệu hóa nút
            txtName.Enabled = false;
            txtAge.Enabled = false;
            cboGender.Enabled = false;
            txtNumber.Enabled = false;
            txtEmail.Enabled = false;
            txtAdress.Enabled = false;

            //Đặt nền trong suất
            txtName.BackColor = this.BackColor;
            txtAge.BackColor = this.BackColor;
            cboGender.BackColor = this.BackColor;

            txtNumber.BackColor = this.BackColor;
            txtEmail.BackColor = this.BackColor;
            txtAdress.BackColor = this.BackColor;

        }
        
        //Hiện thị lại Textbox
        private void ShowTextBox()
        {
            //Hiện viền
            txtName.BorderStyle = BorderStyle.FixedSingle;
            txtAge.BorderStyle = BorderStyle.FixedSingle;
            txtNumber.BorderStyle = BorderStyle.FixedSingle;
            txtEmail.BorderStyle = BorderStyle.FixedSingle;
            txtAdress.BorderStyle = BorderStyle.FixedSingle;

            //Vô hiệu hóa nút
            txtName.Enabled = true;
            txtAge.Enabled = true;
            cboGender.Enabled = true;
            txtNumber.Enabled = true;
            txtEmail.Enabled = true;
            txtAdress.Enabled = true;

            // Đặt lại màu nền mặc định
            txtName.BackColor = SystemColors.Window;
            txtAge.BackColor = SystemColors.Window;
            cboGender.BackColor = SystemColors.Window;
            txtNumber.BackColor = SystemColors.Window;
            txtEmail.BackColor = SystemColors.Window;
            txtAdress.BackColor = SystemColors.Window;
        }

        //chuyển qua chế độ sữa 
        private void FixMode()
        {
            ShowTextBox();
            btnFix.Text = "     Lưu";
            btnFix.Image = Image.FromFile(@"D:\Code\SQL\QuanLyLinhKienMayTinh\Icon\diskette.png");
        }

        //chuyển qua chế độ xem và lưu thông tin
        private void ViewMode()
        {
            UpdateProfileInfo();
            DisplayProfile(CusID);
            HideTextBox();
            btnFix.Text = "     Sữa";
            btnFix.Image = Image.FromFile(@"D:\Code\SQL\QuanLyLinhKienMayTinh\Icon\service.png");
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Click(object sender, EventArgs e)
        {
            OpenChildChildForm(new Payment());
            CustomeInfo = true;
        }

        private void OpenChildChildForm(Form childForm)
        {
            if (mf != null)
            {
                mf.OpenChildChildForm(childForm); // Gọi phương thức của MainForm từ tham chiếu
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(AddUser.UserID.ToString());
        }

        private void txtNumber_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {

        }
    }

}

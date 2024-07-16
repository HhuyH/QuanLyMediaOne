using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static QuanLyLinhKienDIenTu.Login;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace QuanLyLinhKienDIenTu
{
    public partial class AddSanPham : Form
    {
        string strCon = Connecting.GetConnectionString();
        public MainForm mf;
        public static string productid;
        public static bool isAddMode;
        public static bool isViewMode;
        public static bool isEditMode;

        string role = UserSession.role;

        public AddSanPham(MainForm mainForm)
        {
            InitializeComponent();
            BackGroundColor();
            Round.SetSharpCornerPanel(panelAdd);
            mf = mainForm;
            txtPrice.KeyPress += TxtPrice_KeyPress;
        }

        private void TxtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Kiểm tra xem ký tự được nhập vào có phải là chữ không
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                // Nếu là chữ, ngăn việc thêm ký tự đó vào TextBox
                e.Handled = true;
            }
        }

        private void DanhSachKhachHang_Load(object sender, EventArgs e)
        {
            // Đặt lại size của các cột panel để chúng hiển thị lại
            resizePanel();

            // Cập nhật lại layout của TableLayoutPanel
            tableLayoutPanel1.ResumeLayout(true);
            tableLayoutPanel1.PerformLayout();

            this.ControlBox = false;
            LoadCategory();
            if (isAddMode)
            {
                cboDanhMuc.SelectedIndexChanged += CboLinhKien_SelectedIndexChanged;
                EnableBox();

                //chỉnh nút thành chức năng lưu
                btnAdd.Text = "     Lưu";
                btnAdd.Image = Image.FromFile(@"D:\Code\SQL\QuanLyLinhKienMayTinh\Icon\diskette.png");
            }
            else if (isViewMode)
            {
                LoadProductData(productid);
                cboDanhMuc.Enabled = false;
                DisableBox();
                btnAdd.Text = "     Sữa";
                btnAdd.Image = Image.FromFile(@"D:\Code\SQL\QuanLyLinhKienMayTinh\Icon\service.png");
            }
            if(role == "Khach Hang" || role == "Nhan Vien")
            {
                panelAdd.Visible = false;
            }
        }

        //đạt lại size cho table panel
        private void resizePanel()
        {
            // Tổng tỷ lệ phần trăm của tất cả các hàng
            int totalPercentage = 0;

            // Tính tổng số phần trăm của tất cả các hàng
            for (int i = 0; i < tableLayoutPanel1.RowCount; i++)
            {
                totalPercentage += 100; // Ví dụ: bạn có thể thay đổi giá trị này thành phần trăm bạn muốn
            }

            // Đặt SizeType của mỗi hàng thành Percent và đặt phần trăm tương ứng
            for (int i = 0; i < tableLayoutPanel1.RowCount; i++)
            {
                tableLayoutPanel1.RowStyles[i].SizeType = SizeType.Percent;
                tableLayoutPanel1.RowStyles[i].Height = (float)100 / tableLayoutPanel1.RowCount;
            }
        }

        //nút chuyen chuc nang va thuc hien no
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (isAddMode)
            {
                AddMode();
            }
            else if (isViewMode)
            {
                ViewMode();               
            }
            else if (isEditMode)
            {
                EditMode();
            }
        }

        bool SuccAdd = false;
        bool SuccEdit = true;
        //chế độ sữa và lưu sau đó chuyển qua chế độ xem
        private void AddMode()
        {
            //chuc nang add san phẩm
            if (isAddMode)
            {
                InsertProduct();
                if (SuccAdd)
                {
                    //Tải lại dữ liệu vừa được add vào
                    LoadProductData(GetLatestProductID());
                    resizePanel();
                    DisableBox();
                    //Chỉnh lại nút Lưu thành sữa
                    btnAdd.Text = "     Sữa";
                    btnAdd.Image = Image.FromFile(@"D:\Code\SQL\QuanLyLinhKienMayTinh\Icon\service.png");
                    isViewMode = true;
                    isAddMode = false;
                    cboDanhMuc.Enabled = false;
                }
            }
        }

        //chế độ sữa
        private void EditMode()
        {
            if (isEditMode)
            {
                UpdateProduct();
                if (SuccEdit)
                {
                    //Tải lại dữ liệu vừa được add vào
                    LoadProductData(productid);
                    resizePanel();
                    DisableBox();
                    //Chỉnh lại nút Lưu thành sữa
                    btnAdd.Text = "     Sữa";
                    btnAdd.Image = Image.FromFile(@"D:\Code\SQL\QuanLyLinhKienMayTinh\Icon\service.png");
                    isViewMode = true;
                    isEditMode = false;
                }
                else
                {
                    MessageBox.Show("khong the cap nhap");
                }
            }
        }

        //chế độ xem và chuyên qua chế độ sữa
        private void ViewMode()
        {
            EnableBox();
            //chỉnh lại nút sữa thành lưu
            btnAdd.Text = "     Lưu";
            btnAdd.Image = Image.FromFile(@"D:\Code\SQL\QuanLyLinhKienMayTinh\Icon\diskette.png");
            isEditMode = true;
            isViewMode = false;
        }

        //Hàm thêm sản phẩm
        private void InsertProduct()
        {

            // Chuyển đổi giá trị của txtPrice sang decimal
            decimal price = decimal.TryParse(txtPrice.Text, out decimal result) ? result : 0;

            // Chuyển đổi giá trị của txtSL sang int
            int quantity = int.TryParse(txtSL.Text, out int qtyResult) ? qtyResult : 0;

            // Chuyển đổi giá trị của cboDanhMuc.SelectedItem sang int
            int categoryId = int.TryParse(cboDanhMuc.SelectedValue.ToString(), out int catResult) ? catResult : 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(strCon))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("InsertSanPham", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@TENSANPHAM", txtName.Text);
                        cmd.Parameters.AddWithValue("@GIA", price);
                        cmd.Parameters.AddWithValue("@SOLUONG", quantity);
                        cmd.Parameters.AddWithValue("@MADANHMUC", categoryId);

                        cmd.ExecuteNonQuery();
                        SuccAdd = true;
                        MessageBox.Show("Thêm sản phẩm thành công!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm sản phẩm: " + ex.Message);
            }
        }

        //lấy id vùa được add vào
        private string GetLatestProductID()
        {
            string latestProductID = "";

            try
            {
                using (SqlConnection connection = new SqlConnection(strCon))
                {
                    connection.Open();
                    string query = "SELECT TOP 1 MASANPHAM FROM SANPHAM ORDER BY MASANPHAM DESC";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    latestProductID = cmd.ExecuteScalar()?.ToString(); // Chuyển kết quả thành chuỗi
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi lấy ID sản phẩm mới nhất: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return latestProductID;
        }

        private void CboLinhKien_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Đặt lại lai size của các cột để chúng hiển thị lại
            resizePanel();

            // Cập nhật lại layout của TableLayoutPanel
            tableLayoutPanel1.ResumeLayout(true);
            tableLayoutPanel1.PerformLayout();

            //tùy theo loại linh kiện mà hiện thị label và box tương ứng
            //Ẩn ID vi ID được tự tạo
            tableLayoutPanel1.RowStyles[1].Height = 0;
            label2.Visible = false;
            txtID.Visible = false;

        }


        //Bật box lên lại
        private void EnableBox()
        {
            txtPrice.Enabled = true;
            txtSL.Enabled = true;
            txtPrice.Enabled = true;
            txtName.Enabled = true;
            txtID.Enabled = true;
        }

        //Vô hiệu hóa box
        private void DisableBox()
        {
            txtPrice.Enabled = false;
            txtSL.Enabled = false;
            txtPrice.Enabled = false;
            txtName.Enabled = false;
            txtID.Enabled = false;
        }

        //ẩn tất cả các giá trị trước khi chọn component type
        private void HideBox()
        {
            //ẩn tên
            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            label5.Visible = false;

            //ẩn textbox và combobox
            txtPrice.Visible = false;
            txtSL.Visible = false;
            txtPrice.Visible = false;
            txtName.Visible = false;
            txtID.Visible = false;
        }
        
        //hiện thị tất cả giá trị
        private void ShowBox()
        {
            // Hiển thị các nhãn
            label2.Visible = true;
            label3.Visible = true;
            label4.Visible = true;
            label5.Visible = true;

            // Hiển thị các textbox và combobox
            txtPrice.Visible = true;
            txtSL.Visible = true;
            txtPrice.Visible = true;
            txtName.Visible = true;
            txtID.Visible = true;
        }

        //tải sản phẩm
        private void LoadProductData(string productId)
        {
            // Sử dụng câu lệnh using để đảm bảo rằng SqlConnection được giải phóng đúng cách ngay cả khi có ngoại lệ xảy ra
            using (SqlConnection con = new SqlConnection(strCon))
            {
                try
                {
                    con.Open();

                    string query = "SELECT S.MASANPHAM, S.TENSANPHAM, S.GIA, S.SOLUONG, DM.TENDANHMUC " +
                                "FROM SANPHAM S INNER JOIN DANHMUCSANPHAM DM ON S.MADANHMUC = DM.MADANHMUC " +
                                "WHERE MASANPHAM = @productid";
                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@productid", productId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            cboDanhMuc.SelectedItem = reader["TENDANHMUC"].ToString();
                            txtID.Text = reader["MASANPHAM"].ToString();
                            txtName.Text = reader["TENSANPHAM"].ToString();
                            txtPrice.Text = reader["GIA"].ToString();
                            txtSL.Text = reader["SOLUONG"].ToString();

                        }
                    }
                    else
                    {
                        // Không có dữ liệu được trả về từ câu truy vấn
                        MessageBox.Show("Không tìm thấy sản phẩm có ID: " + productId);
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý ngoại lệ nếu có
                    MessageBox.Show("Đã xảy ra lỗi: " + ex.Message);
                }
            }
        }


        // Hàm cập nhật sản phẩm
        private void UpdateProduct()
        {
            // Chuyển đổi giá trị của txtPrice sang decimal
            decimal price = decimal.TryParse(txtPrice.Text, out decimal result) ? result : 0;

            // Chuyển đổi giá trị của txtSL sang int
            int quantity = int.TryParse(txtSL.Text, out int qtyResult) ? qtyResult : 0;

            // Chuyển đổi giá trị của cboDanhMuc.SelectedValue sang int
            int categoryId = int.TryParse(cboDanhMuc.SelectedValue.ToString(), out int catResult) ? catResult : 0;

            // Chuyển đổi giá trị của txtID (mã sản phẩm) sang int
            int productId = int.TryParse(txtID.Text, out int idResult) ? idResult : 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(strCon))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("UpdateSanPham", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@MASANPHAM", productId);
                        cmd.Parameters.AddWithValue("@TENSANPHAM", txtName.Text);
                        cmd.Parameters.AddWithValue("@GIA", price);
                        cmd.Parameters.AddWithValue("@SOLUONG", quantity);
                        cmd.Parameters.AddWithValue("@MADANHMUC", categoryId);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Cập nhật sản phẩm thành công!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật sản phẩm: " + ex.Message);
            }
        }

        //chỉnh màu nền
        private void BackGroundColor()
        {
            MainForm mf = new MainForm();

            this.BackColor = mf.GetPanelBodyColor();
        }


        //Tải lên loại linh kiện
        private void LoadCategory()
        {
            using (SqlConnection con = new SqlConnection(strCon))
            {
                try
                {
                    con.Open();
                    string query = "SELECT MADANHMUC, TENDANHMUC FROM DANHMUCSANPHAM";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            cboDanhMuc.DataSource = dt;
                            cboDanhMuc.DisplayMember = "TENDANHMUC";
                            cboDanhMuc.ValueMember = "MADANHMUC";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Đã xảy ra lỗi: " + ex.Message);
                }
            }
        }


        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cboLinhKien_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void btnUpdImage_Click(object sender, EventArgs e)
        {

        }

        private void txtPrice_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            // Kiểm tra xem ký tự được nhập vào có phải là một số hoặc phím Backspace không
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                // Nếu không phải là số hoặc phím Backspace, hủy sự kiện KeyPress
                e.Handled = true;
                // Hiển thị một cảnh báo cho người dùng
                MessageBox.Show("Vui lòng chỉ nhập số.");
            }
        }

        private void txtSL_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Kiểm tra xem ký tự được nhập vào có phải là một số hoặc phím Backspace không
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                // Nếu không phải là số hoặc phím Backspace, hủy sự kiện KeyPress
                e.Handled = true;
                // Hiển thị một cảnh báo cho người dùng
                MessageBox.Show("Vui lòng chỉ nhập số.");
            }
        }
    }
}

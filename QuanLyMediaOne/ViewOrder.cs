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
    public partial class ViewOrder : Form
    {
        string strCon = Connecting.GetConnectionString();
        public MainForm mf;
        public static string productid;
        public static bool isAddMode;
        string role = UserSession.role;
        int UserID = UserSession.UserId;

        public ViewOrder(MainForm mainForm)
        {
            InitializeComponent();
            BackGroundColor();

            mf = mainForm;
        }

        private void Order_Load(object sender, EventArgs e)
        {
            if(role == "Customer")
            {
                LoadOrdersForCustomer(UserID);
                panelAddCart.Visible = false;
            }
            else
            {
                LoadOrdersForStaff();
            }
            DataGrindview();
            Round.SetSharpCornerPanel(panelAddCart);
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
            txtSearch.TextChanged += TxtSearch_TextChanged;
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            // Lấy nội dung tìm kiếm từ TextBox
            string keyword = txtSearch.Text.Trim();

            // Gọi phương thức SearchItems với từ khóa tìm kiếm
            Search(keyword);
        }

        private void Search(string keyword)
        {
            // Tạo câu truy vấn SQL tìm kiếm
            string query = "SELECT MADONHANG, MAKH, NGAYLAPDONHAN, TONGGIATIEN, NGUOINHAN, DIACHIGIAO, " +
                         "TRANGTHAIHUY, TRANGTHAITHANHTOAN, TRANGTHAIXACNHAN " +
                         "FROM DONHANG " +
                         "WHERE MADONHANG LIKE @Keyword " +
                         "OR MAKH LIKE @Keyword " +
                         "OR NGAYLAPDONHAN LIKE @Keyword " +
                         "OR TONGGIATIEN LIKE @Keyword " +
                         "OR NGUOINHAN LIKE @Keyword " +
                         "OR DIACHIGIAO LIKE @Keyword " +
                         "OR TRANGTHAIHUY LIKE @Keyword " +
                         "OR TRANGTHAITHANHTOAN LIKE @Keyword " +
                         "OR TRANGTHAIXACNHAN LIKE @Keyword";


            // Tạo kết nối đến cơ sở dữ liệu
            using (SqlConnection connection = new SqlConnection(strCon))
            {
                // Mở kết nối
                connection.Open();

                // Tạo đối tượng SqlCommand
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Thêm tham số cho từ khóa tìm kiếm
                    command.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                    // Tạo đối tượng SqlDataAdapter để lấy dữ liệu từ SQL Server và đổ vào DataTable
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        // Khởi tạo DataTable để lưu trữ dữ liệu
                        DataTable dt = new DataTable();

                        // Đổ dữ liệu từ SqlDataAdapter vào DataTable
                        adapter.Fill(dt);

                        // Đặt DataTable làm nguồn dữ liệu cho DataGridView
                        dataGridView1.DataSource = dt;
                    }
                }
            }
        }


        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Lấy giá trị của cột ID
                DetailOrder.OrderID = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[0].Value);
                mf.OpenChildChildForm(new DetailOrder(mf));
            }
        }

        //Chỉnh datagrindvie
        private void DataGrindview()
        {
            // Ẩn hàng dự thêm mới ở cuối DataGridView
            dataGridView1.AllowUserToAddRows = false;
            // Đặt DataGridView thành chỉ chế độ chỉ đọc
            dataGridView1.ReadOnly = true;
            dataGridView1.BackgroundColor = this.BackColor;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.RowHeadersVisible = false;
        }


        //tải hóa đơn cho nhân viên
        private void LoadOrdersForStaff()
        {
            string query = "SELECT D.MADONHANG, D.MAKH, D.NGUOINHAN, D.DIACHIGIAO, D.TRANGTHAITHANHTOAN, D.TRANGTHAIXACNHAN, D.TRANGTHAIHUY, D.NGAYLAPDONHAN, D.TONGGIATIEN " +
                           "FROM DONHANG D";


            using (SqlConnection connection = new SqlConnection(strCon))
            {
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();

                try
                {
                    connection.Open();
                    adapter.Fill(dataTable);

                    // Đặt dữ liệu vào DataGridView
                    dataGridView1.DataSource = dataTable;

                    dataGridView1.Columns["MADONHANG"].HeaderText = "STT";
                    dataGridView1.Columns["MAKH"].HeaderText = "Khách hàng";
                    dataGridView1.Columns["TONGGIATIEN"].HeaderText = "Giá";
                    dataGridView1.Columns["NGUOINHAN"].HeaderText = "Tên người nhận";
                    dataGridView1.Columns["DIACHIGIAO"].HeaderText = "Địa chỉ nhận";
                    dataGridView1.Columns["TRANGTHAITHANHTOAN"].HeaderText = "Trạng thái thanh toán";
                    dataGridView1.Columns["NGAYLAPDONHAN"].HeaderText = "Ngày đặt";
                    dataGridView1.Columns["TRANGTHAIXACNHAN"].HeaderText = "Trạng thái xác nhận";
                    dataGridView1.Columns["TRANGTHAIHUY"].HeaderText = "Trạng thái";
                    // Căn giữa các cột
                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Errorgaw: " + ex.Message);
                }
            }
        }

        private void LoadOrdersForCustomer(int UserID)
        {
            string query = "SELECT MADONHANG, MAKH, NGAYLAPDONHAN, TONGGIATIEN, NGUOINHAN, DIACHIGIAO, " +
                           "TRANGTHAIHUY, TRANGTHAITHANHTOAN, TRANGTHAIXACNHAN " +
                           "FROM DONHANG" +
                           "INNER JOIN KHACHHANG ON DONHANG.MAKH = KHACHHANG.MAKH " +
                           "WHERE KHACHHANG.MAKH = @UserID";


            using (SqlConnection connection = new SqlConnection(strCon))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserID", UserID);

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();

                try
                {
                    connection.Open();
                    adapter.Fill(dataTable);

                    // Đặt dữ liệu vào DataGridView
                    dataGridView1.DataSource = dataTable;

                    dataGridView1.Columns["MADONHANG"].HeaderText = "STT";
                    dataGridView1.Columns["MAKH"].HeaderText = "Khách hàng";
                    dataGridView1.Columns["TONGGIATIEN"].HeaderText = "Giá";
                    dataGridView1.Columns["NGUOINHAN"].HeaderText = "Tên người nhận";
                    dataGridView1.Columns["DIACHIGIAO"].HeaderText = "Địa chỉ nhận";
                    dataGridView1.Columns["TRANGTHAITHANHTOAN"].HeaderText = "Trạng thái thanh toán";
                    dataGridView1.Columns["NGAYLAPDONHAN"].HeaderText = "Ngày đặt";
                    dataGridView1.Columns["TRANGTHAIXACNHAN"].HeaderText = "Trạng thái xác nhận";
                    dataGridView1.Columns["TRANGTHAIHUY"].HeaderText = "Trạng thái";

                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("lỗi: " + ex.Message);
                }
            }
        }

        //chỉnh màu nền
        private void BackGroundColor()
        {
            MainForm mf = new MainForm();

            this.BackColor = mf.GetPanelBodyColor();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            mf.OpenChildForm(new Order(mf));
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

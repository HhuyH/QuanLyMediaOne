using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QuanLyLinhKienDIenTu.Login;

namespace QuanLyLinhKienDIenTu
{
    public partial class DetailOrder : Form
    {
        string strCon = Connecting.GetConnectionString();
        public static int OrderID;
        public MainForm mf;
        int UserID = UserSession.UserId;
        string role = UserSession.role;

        public DetailOrder(MainForm mainForm)
        {
            InitializeComponent();
            BackGroundColor();
            mf = mainForm;
            DataGrindview();
            UserRole();
            Status();
        }

        //Kiểm tra người dùng
        private void UserRole()
        {
            if(role == "Khach Hang")
            {
                panelAcc.Visible = false;
                panel7.Visible = false;
                panel5.Visible = true;
                panelOrder.Visible = true;
            }
            else if (role != "Nhan Vien")
            {
                panelAcc.Visible = true;
                panel7.Visible = true;
                panel5.Visible = false;
                panelOrder.Visible = false;
            }
            else if(role == "Nhan Vien")
            {
                panel5.Visible = false;
                panelOrder.Visible = false;
                panelAcc.Visible = false;
                panel7.Visible = false;
            }

        }
        private void DanhSachKhachHang_Load(object sender, EventArgs e)
        {
            this.ControlBox = false;
            LoadDetailOrders(OrderID);
            LoadOrders(OrderID);
            this.Width = 975;
        }

        //Xác nhận trạng thái
        private void Status()
        {
            if (AbortST == "Hủy")
            {
                panelAcc.Visible = false;
                panel7.Visible = false;
            }
            else if (AccST == "Xác nhận")
            {
                panel5.Visible = false;
                panelOrder.Visible = false;
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

        //chỉnh màu nền
        private void BackGroundColor()
        {
            MainForm mf = new MainForm();
            dataGridView1.BackgroundColor = mf.GetPanelBodyColor();
            dataGridView1.ReadOnly = true;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.RowHeadersVisible = false;
            this.BackColor = mf.GetPanelBodyColor();
        }

        //tải chi tiết hóa đơn
        private void LoadDetailOrders(int OrderID)
        {
            using (SqlConnection connection = new SqlConnection(strCon))
            {
                string query = "SELECT S.MASANPHAM, S.TENSANPHAM, CT.GIATIEN, CT.GIAMGIA, CT.SOLUONG " +
                               "FROM DONHANG D " +
                               "INNER JOIN CHITIETDONHAN CT ON D.MADONHANG = CT.MADONHANG " +
                               "INNER JOIN SANPHAM S ON S.MASANPHAM = CT.MASANPHAM " +
                               "WHERE D.MADONHANG = @OrderID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text; // Chỉnh lại CommandType thành Text

                    command.Parameters.AddWithValue("@OrderID", OrderID);

                    connection.Open();

                    // Thực thi câu lệnh SQL và lấy dữ liệu vào DataTable
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Đặt dữ liệu vào DataGridView
                    dataGridView1.DataSource = dataTable;

                    dataGridView1.Columns["MASANPHAM"].HeaderText = "Mã sản phẩm";
                    dataGridView1.Columns["TENSANPHAM"].HeaderText = "Tên sản phẩm";
                    dataGridView1.Columns["GIATIEN"].HeaderText = "Giá";
                    dataGridView1.Columns["GIAMGIA"].HeaderText = "Ưu đãi";
                    dataGridView1.Columns["SOLUONG"].HeaderText = "Số lượng";

                    // Căn giữa các cột
                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }

                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
        }

        string AbortST;
        string AccST;
        //tải hóa đơn
        private void LoadOrders(int OrderID)
        {
            string query = "SELECT NGAYLAPDONHAN, TONGGIATIEN, " +
                "DIACHIGIAO, TRANGTHAIHUY, TRANGTHAIXACNHAN " +
                "FROM DONHANG WHERE MADONHANG = @OrderID";

            using (SqlConnection connection = new SqlConnection(strCon))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderID", OrderID);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Đặt dữ liệu vào các Label
                            lbOrderDate.Text = "Ngày đặt hàng: " + reader["NGAYLAPDONHAN"].ToString();
                            lbPrice.Text = "Tổng giá: " + reader["TONGGIATIEN"].ToString();
                            lbAdress.Text = reader["DIACHIGIAO"].ToString();
                            lbAbortSt.Text = "Trạng thái: " + reader["TRANGTHAIHUY"].ToString();
                            lbAcceptSt.Text = "Trạng thái Xác nhận: " + reader["TRANGTHAIXACNHAN"].ToString();
                            AbortST = reader["TRANGTHAIHUY"].ToString();
                            AccST = reader["TRANGTHAIXACNHAN"].ToString();

                            if (AbortST == "Hủy" || AccST == "Xác nhận")
                            {
                                panelAcc.Visible = false;
                                panel7.Visible = false;
                                panel5.Visible = false;
                                panelOrder.Visible = false;
                            }
                        }
                        else
                        {
                            // Xử lý trường hợp không tìm thấy dữ liệu
                            MessageBox.Show("Không tìm thấy đơn hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void panel2_Click(object sender, EventArgs e)
        {
            this.Close();
            mf.OpenChildForm(new ViewOrder(mf));
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        //Cập nhập số lượng
        private void UpdateQuantityInDatabase(int productId, int newQuantity)
        {
            // Tạo câu lệnh SQL để cập nhật số lượng trong bảng Products
            string query = "UPDATE SANPHAM SET SOLUONG = @NewQuantity WHERE MASANPHAM = @ProductId";

            // Tạo kết nối đến cơ sở dữ liệu
            using (SqlConnection con = new SqlConnection(strCon))
            {
                try
                {
                    // Mở kết nối
                    con.Open();

                    // Tạo đối tượng SqlCommand
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Thêm tham số cho câu lệnh SQL
                        cmd.Parameters.AddWithValue("@NewQuantity", newQuantity);
                        cmd.Parameters.AddWithValue("@ProductId", productId);

                        // Thực thi câu lệnh SQL
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Đã xảy ra lỗi khi cập nhật số lượng sản phẩm: " + ex.Message);
                }
            }
        }
        private void btnAccAbort_Click(object sender, EventArgs e)
        {
            // Hiển thị hộp thoại xác nhận
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn xác nhận đơn hàng không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // Kiểm tra kết quả của hộp thoại xác nhận
            if (result == DialogResult.Yes)
            {
                // Nếu người dùng chọn "Yes", thực hiện cập nhật trạng thái đơn hàng
                UpdateOrderStatus(OrderID, "Chưa xác nhận", "Hủy");
                LoadOrders(OrderID);
            }
        }

        bool UpdSL;

        private void BtnAcc_Click(object sender, EventArgs e)
        {
            // Hiển thị hộp thoại xác nhận
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn xác nhận đơn hàng không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // Kiểm tra kết quả của hộp thoại xác nhận
            if (result == DialogResult.Yes)
            {

                // Đặt biến UpdSL để kiểm tra trạng thái cập nhật số lượng
                bool UpdSL = UpdateProductQuantities(OrderID);

                // Chỉ khi cập nhật số lượng thành công, mới tiếp tục tải lại thông tin đơn hàng
                if (UpdSL)
                {
                    // Nếu người dùng chọn "Yes", thực hiện cập nhật trạng thái đơn hàng
                    UpdateOrderStatus(OrderID, "Xác nhận", "Chưa hủy");
                    LoadOrders(OrderID);
                }
                else
                {
                    MessageBox.Show("Cập nhật số lượng sản phẩm không thành công, vui lòng thử lại.");
                }
            }
        }

        // Phương thức cập nhật số lượng sản phẩm trong bảng SANPHAM
        private bool UpdateProductQuantities(int orderId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(strCon))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        string updateQuery = @"UPDATE SANPHAM 
                       SET SOLUONG = SP.SOLUONG - CT.SOLUONG
                       FROM SANPHAM SP
                       INNER JOIN CHITIETDONHAN CT ON SP.MASANPHAM = CT.MASANPHAM
                       WHERE CT.MADONHANG = @order_id";

                        SqlCommand command = new SqlCommand(updateQuery, connection, transaction);

                        // Điền thông tin cho tham số
                        command.Parameters.AddWithValue("@order_id", orderId);

                        // In ra câu lệnh SQL và giá trị tham số để kiểm tra
                        Console.WriteLine("Query: " + updateQuery);
                        Console.WriteLine("Order ID: " + orderId);

                        command.ExecuteNonQuery();
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Đã xảy ra lỗi khi cập nhật số lượng sản phẩm: " + ex.Message + "\n" + ex.StackTrace);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi kết nối cơ sở dữ liệu: " + ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }


        //chuyền trạng thái hủy từ khách hàng hoặc xác nhận từ shop đơn hàng

        public void UpdateOrderStatus(int orderId, string acceptStatus, string cancelStatus)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(strCon))
                {
                    connection.Open();

                    string updateQuery = @"UPDATE DONHANG 
                                           SET TRANGTHAIXACNHAN = @accept_status, TRANGTHAIHUY = @cancel_status 
                                           WHERE MADONHANG = @order_id";

                    SqlCommand command = new SqlCommand(updateQuery, connection);

                    // Điền thông tin cho các tham số
                    command.Parameters.AddWithValue("@accept_status", acceptStatus);
                    command.Parameters.AddWithValue("@cancel_status", cancelStatus);
                    command.Parameters.AddWithValue("@order_id", orderId);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}

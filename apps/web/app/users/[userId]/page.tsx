"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { userService, UserTransferObject, UpdateUserProfileRequest } from "../../../services/user";
import { AlertCircle, Edit, User as UserIcon, Calendar, Globe, DollarSign, Activity, ArrowLeft } from "lucide-react";
import Link from "next/link";

export default function UserDetailsPage() {
  const params = useParams<{ userId: string }>();
  const [user, setUser] = useState<UserTransferObject | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [isEditOpen, setIsEditOpen] = useState(false);
  const [editData, setEditData] = useState<UpdateUserProfileRequest>({
    firstName: "",
    lastName: "",
    phoneNumber: "",
    dateOfBirth: "",
    currency: "",
    timeZone: "",
    language: ""
  });

  const fetchUser = async () => {
    setLoading(true);
    try {
      if (!params || !params.userId) return;
      const res = await userService.getUserById(params.userId);
      if (res.success && res.data) {
        setUser(res.data);
        const p = res.data.profile;
        setEditData({
          firstName: res.data.firstName || "",
          lastName: res.data.lastName || "",
          phoneNumber: res.data.phoneNumber || "",
          dateOfBirth: p?.dateOfBirth ? p.dateOfBirth.split('T')[0] : "",
          currency: p?.currency || "",
          timeZone: p?.timeZone || "",
          language: p?.language || "",
        });
      } else {
        setError(res.message || "User not found");
      }
    } catch (err: any) {
      setError(err.message || "Failed to fetch user");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (params?.userId) fetchUser();
  }, [params?.userId]);

  const handleUpdate = async () => {
    if (!user) return;
    try {
      const payload: UpdateUserProfileRequest = {
        firstName: editData.firstName,
        lastName: editData.lastName,
        phoneNumber: editData.phoneNumber,
        dateOfBirth: editData.dateOfBirth ? new Date(editData.dateOfBirth).toISOString() : null,
        currency: editData.currency,
        timeZone: editData.timeZone,
        language: editData.language,
      };
      
      const res = await userService.updateProfile(user.id, payload);
      if (res.success) {
        setIsEditOpen(false);
        fetchUser();
      } else {
        alert(res.message || "Failed to update profile");
      }
    } catch (err: any) {
      alert(err.message || "Failed to update profile");
    }
  };

  if (loading) return <div className="p-8 text-center text-muted-foreground">Loading user details...</div>;
  if (error || !user) {
    return (
      <div className="p-8">
        <div className="bg-destructive/10 text-destructive p-4 rounded-md flex items-center gap-2">
          <AlertCircle className="w-5 h-5" />
          {error || "User not found"}
        </div>
      </div>
    );
  }

  const p = user.profile;

  return (
    <div className="p-8 max-w-4xl mx-auto space-y-8">
      <div className="flex items-center gap-4">
        <Link href="/users" passHref>
          <Button variant="outline" size="icon" className="shrink-0"><ArrowLeft className="w-4 h-4" /></Button>
        </Link>
        <div className="flex-1">
          <h1 className="text-3xl font-bold tracking-tight">User Details</h1>
          <p className="text-muted-foreground mt-1">Viewing profile for {user.fullName || user.email}</p>
        </div>
        
        <Dialog open={isEditOpen} onOpenChange={setIsEditOpen}>
          <DialogTrigger asChild>
            <Button className="gap-2"><Edit className="w-4 h-4" /> Edit Profile</Button>
          </DialogTrigger>
          <DialogContent className="max-w-md">
            <DialogHeader>
              <DialogTitle>Edit Profile</DialogTitle>
              <DialogDescription>Update the user's personal and profile information.</DialogDescription>
            </DialogHeader>
            <div className="py-4 space-y-4 max-h-[60vh] overflow-y-auto">
              <div className="grid grid-cols-2 gap-4">
                 <div className="space-y-2">
                   <Label>First Name</Label>
                   <Input value={editData.firstName || ""} onChange={e => setEditData({ ...editData, firstName: e.target.value })} />
                 </div>
                 <div className="space-y-2">
                   <Label>Last Name</Label>
                   <Input value={editData.lastName || ""} onChange={e => setEditData({ ...editData, lastName: e.target.value })} />
                 </div>
              </div>
              <div className="space-y-2">
                <Label>Phone Number</Label>
                <Input value={editData.phoneNumber || ""} onChange={e => setEditData({ ...editData, phoneNumber: e.target.value })} />
              </div>
              <div className="space-y-2">
                <Label>Date of Birth</Label>
                <Input type="date" value={editData.dateOfBirth || ""} onChange={e => setEditData({ ...editData, dateOfBirth: e.target.value })} />
              </div>
              <div className="grid grid-cols-2 gap-4">
                 <div className="space-y-2">
                   <Label>Currency</Label>
                   <Input value={editData.currency || ""} onChange={e => setEditData({ ...editData, currency: e.target.value })} placeholder="USD" />
                 </div>
                 <div className="space-y-2">
                   <Label>Language</Label>
                   <Input value={editData.language || ""} onChange={e => setEditData({ ...editData, language: e.target.value })} placeholder="en-US" />
                 </div>
              </div>
              <div className="space-y-2">
                <Label>Time Zone</Label>
                <Input value={editData.timeZone || ""} onChange={e => setEditData({ ...editData, timeZone: e.target.value })} placeholder="UTC" />
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsEditOpen(false)}>Cancel</Button>
              <Button onClick={handleUpdate}>Save Changes</Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card className="shadow-sm">
          <CardHeader>
            <CardTitle className="flex items-center gap-2"><UserIcon className="w-5 h-5 text-primary" /> Basic Info</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4 text-sm">
            <div className="flex justify-between border-b pb-2">
              <span className="text-muted-foreground">ID</span>
              <span className="font-mono text-xs text-right max-w-[200px] truncate" title={user.id}>{user.id}</span>
            </div>
            <div className="flex justify-between border-b pb-2">
              <span className="text-muted-foreground">Email</span>
              <span className="font-medium text-right max-w-[200px] truncate" title={user.email || ""}>{user.email || "N/A"}</span>
            </div>
            <div className="flex justify-between border-b pb-2">
              <span className="text-muted-foreground">Username</span>
              <span>{user.userName || "N/A"}</span>
            </div>
            <div className="flex justify-between border-b pb-2">
              <span className="text-muted-foreground">Phone</span>
              <span>{user.phoneNumber || "N/A"}</span>
            </div>
            <div className="flex justify-between pb-2">
              <span className="text-muted-foreground">Roles</span>
              <span>{user.roles?.join(', ') || "User"}</span>
            </div>
          </CardContent>
        </Card>

        <Card className="shadow-sm">
          <CardHeader>
             <CardTitle className="flex items-center gap-2"><Activity className="w-5 h-5 text-primary" /> Profile Settings</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4 text-sm">
            <div className="flex justify-between border-b pb-2">
              <span className="flex items-center gap-2 text-muted-foreground"><Calendar className="w-4 h-4" /> Date of Birth</span>
              <span>{p?.dateOfBirth ? new Date(p.dateOfBirth).toLocaleDateString() : "N/A"} {p?.age ? `(${p.age} yrs)` : ""}</span>
            </div>
            <div className="flex justify-between border-b pb-2">
              <span className="flex items-center gap-2 text-muted-foreground"><DollarSign className="w-4 h-4" /> Currency</span>
              <span>{p?.currency || "N/A"}</span>
            </div>
            <div className="flex justify-between border-b pb-2">
              <span className="flex items-center gap-2 text-muted-foreground"><Globe className="w-4 h-4" /> Language</span>
              <span>{p?.language || "N/A"}</span>
            </div>
            <div className="flex justify-between pb-2">
              <span className="text-muted-foreground">Time Zone</span>
              <span>{p?.timeZone || "N/A"}</span>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
